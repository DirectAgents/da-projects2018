using System;
using System.Linq;
using EomTool.Domain.Entities;

namespace EomTool.Domain.Concrete
{
    public class PublisherRelatedItemsRepository : IDisposable
    {
        EomEntities context;

        public PublisherRelatedItemsRepository(EomEntities context)
        {
            this.context = context;
        }

        public IQueryable<PubNote> Notes(string publisherName)
        {
            var result = this.context.PubNotes.Where(c => c.publisher_name == publisherName);
            return result;
        }

        public void AddNote(string publisherName, string authorIdentity, string noteText)
        {
            var note = new PubNote()
            {
                note = noteText,
                created = DateTime.Now,
                publisher_name = publisherName,
                added_by_system_user = authorIdentity
            };
            context.PubNotes.Add(note);
        }

        public IQueryable<PubAttachment> Attachments(string publisherName)
        {
            var result = this.context.PubAttachments.Where(c => c.publisher_name == publisherName);
            return result;
        }

        public void AddAttachment(string publisherName, string attachmentName, string attachmentDescription, byte[] binaryContent)
        {
            var attachment = new PubAttachment
            {
                publisher_name = publisherName,
                name = attachmentName,
                description = attachmentDescription,
                binary_content = binaryContent
            };
            context.PubAttachments.Add(attachment);
        }

        public PubAttachment AttachmentContentById(int attachmentID)
        {
            return context.PubAttachments.Single(c => c.id == attachmentID);
        }

        public void Dispose()
        {
        }
    }
}
