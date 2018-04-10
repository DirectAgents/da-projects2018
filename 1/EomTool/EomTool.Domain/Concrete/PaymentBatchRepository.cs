using System.Linq;
using EomTool.Domain.Abstract;
using EomTool.Domain.Entities;

namespace EomTool.Domain.Concrete
{
    public class PaymentBatchRepository : IPaymentBatchRepository
    {
        EomEntities context;

        public PaymentBatchRepository(EomEntities context)
        {
            this.context = context;
        }

        public IQueryable<PaymentBatch> PaymentBatches
        {
            get { return context.PaymentBatches; }
        }

        // if identity==null, will return everyone's batches
        // if batchState < 0, won't filter by batchState
        public IQueryable<PaymentBatch> PaymentBatchesForUser(string identity, int batchState)
        {
            var batches = context.PaymentBatches as IQueryable<PaymentBatch>;
            if (identity != null)
                batches = batches.Where(b => b.approver_identity == identity);
            if (batchState >= 0)
                batches = batches.Where(pb => pb.payment_batch_state_id == batchState);
            return batches;
        }

        public IQueryable<PublisherPayment> PublisherPayments
        {
            get { return context.PublisherPayments; }
        }

        public IQueryable<PublisherPayment> PublisherPaymentsForUser(string identity, int batchState)
        {
            var batches = PaymentBatchesForUser(identity, batchState);
            var batchIds = batches.Select(b => b.id).ToList();

            return context.PublisherPayments.Where(p => p.PaymentBatchId != null && batchIds.Contains(p.PaymentBatchId.Value));
        }

        // These are separate payouts (by campaign) used in publisher reports...
        public IQueryable<PublisherPayout> PublisherPayouts
        {
            get { return context.PublisherPayouts; }
        }

        public IQueryable<PaymentBatch> PaymentBatchesForItemIds(int[] itemIds)
        {
            var batches = context.Items.Where(item => itemIds.Contains(item.id)).Select(item => item.PaymentBatch).Distinct();
            return batches;
        }

        // returns true iff a batchState is changed from not complete to complete
        public bool CheckIfBatchesComplete(int[] itemIds)
        {
            bool retval = false;
            var batches = PaymentBatchesForItemIds(itemIds);
            foreach (var batch in batches)
            {
                bool isComplete = !(batch.Items.Any(item => item.item_accounting_status_id != ItemAccountingStatus.CheckSignedAndPaid &&
                                                            item.item_accounting_status_id != ItemAccountingStatus.Hold));
                if (isComplete)
                {
                    if (batch.payment_batch_state_id != PaymentBatchState.Complete)
                    {
                        batch.payment_batch_state_id = PaymentBatchState.Complete;
                        retval = true;
                    }
                }
                else if (batch.payment_batch_state_id == PaymentBatchState.Complete)
                    batch.payment_batch_state_id = PaymentBatchState.Default;
            }
            context.SaveChanges();
            return retval;
        }

        // --- Actions ---

        public void SetAccountingStatus(int[] itemIds, int accountingStatus)
        {
            var items = context.Items.Where(item => itemIds.Contains(item.id));
            foreach (var item in items)
            {
                item.item_accounting_status_id = accountingStatus;
            }
            context.SaveChanges();
        }

        public void SetPaymentBatchId(int[] itemIds, int paymentBatchId)
        {
            var items = context.Items.Where(item => itemIds.Contains(item.id));
            foreach (var item in items)
            {
                item.payment_batch_id = paymentBatchId;
            }
            context.SaveChanges();
        }

        // --- Notes ---

        public IQueryable<PubNote> PubNotes
        {
            get { return context.PubNotes; }
        }

        public IQueryable<PubNote> PubNotesForPublisher(string pubName)
        {
            return context.PubNotes.Where(n => n.publisher_name == pubName);
        }

        public void AddPubNote(string pubName, string note, string identity)
        {
            var pubNote = new PubNote()
            {
                note = note,
                added_by_system_user = identity,
                publisher_name = pubName
            };
            context.PubNotes.Add(pubNote);
            context.SaveChanges();
        }

        // --- Attachments ---

        public IQueryable<PubAttachment> PubAttachments
        {
            get { return context.PubAttachments; }
        }

        public IQueryable<PubAttachment> PubAttachmentsForPublisher(string pubName)
        {
            return context.PubAttachments.Where(a => a.publisher_name == pubName);
        }

    }
}
