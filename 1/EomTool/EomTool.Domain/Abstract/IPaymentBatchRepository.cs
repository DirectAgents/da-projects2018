using System.Linq;
using EomTool.Domain.Entities;

namespace EomTool.Domain.Abstract
{
    public interface IPaymentBatchRepository
    {
        IQueryable<PaymentBatch> PaymentBatches { get; }
        IQueryable<PaymentBatch> PaymentBatchesForUser(string identity, int batchState);

        IQueryable<PublisherPayment> PublisherPayments { get; }
        IQueryable<PublisherPayment> PublisherPaymentsForUser(string identity, int batchState);

        IQueryable<PublisherPayout> PublisherPayouts { get; }

        IQueryable<PaymentBatch> PaymentBatchesForItemIds(int[] itemIds);
        bool CheckIfBatchesComplete(int[] itemIds);

        void SetAccountingStatus(int[] itemIds, int accountingStatus);
        void SetPaymentBatchId(int[] itemIds, int paymentBatchId);

        IQueryable<PubNote> PubNotes { get; }
        IQueryable<PubNote> PubNotesForPublisher(string pubName);
        void AddPubNote(string pubName, string note, string identity);

        IQueryable<PubAttachment> PubAttachments { get; }
        IQueryable<PubAttachment> PubAttachmentsForPublisher(string pubName);
    }
}
