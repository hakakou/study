using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace TodoApp.Entities
{
    public class TodoItem : BasicAggregateRoot<Guid>
    {
        public string Text { get; set; } = string.Empty;
    }

    public class IssueLabel : Entity
    {
        public virtual Guid IssueId { get; private set; }
        public virtual Guid LabelId { get; private set; }

        protected IssueLabel()
        {
        }

        public IssueLabel(Guid issueId, Guid labelId)
        {
            IssueId = issueId;
            LabelId = labelId;
        }

        public override object?[] GetKeys()
        {
            return [IssueId, LabelId];
        }
    }

    public enum IssueCloseReason
    {
        Resolved = 1,
        Duplicate = 2,
        WontFix = 3,
        Invalid = 4
    }

    public class Issue : FullAuditedAggregateRoot<Guid> //Using Guid as the key/identifier
    {
        public virtual string Title { get; private set; } //Changed using the SetTitle() method
        public virtual string Text { get; set; } //Can be directly changed. null values are allowed
        public virtual Guid? MilestoneId { get; set; } //Reference to another aggregate root
        public virtual bool IsClosed { get; private set; }
        public virtual IssueCloseReason? CloseReason { get; private set; } //Just an enum type
        public virtual Collection<IssueLabel> Labels { get; protected set; } //Sub collection

        protected Issue()
        {
            /* This constructor is for ORMs to be used while getting the entity from database.
             * - No need to initialize the Labels collection
                 since it will be overrided from the database.
               - It's protected since proxying and deserialization tools
                 may not work with private constructors.
             */
        }

        //Primary constructor
        public Issue(
            Guid id, //Get Guid value from the calling code
            [NotNull] string title, //Indicate that the title can not be null.
            string text = null,
            Guid? milestoneId = null) //Optional argument
        {
            Id = id;
            Title = Check.NotNullOrWhiteSpace(title, nameof(title)); //Validate
            Text = text;
            MilestoneId = milestoneId;

            Labels = new Collection<IssueLabel>(); //Always initialize the collection
        }

        public virtual Issue SetTitle([NotNull] string title)
        {
            Title = Check.NotNullOrWhiteSpace(title, nameof(title)); //Validate
            return this;
        }

        /* AddLabel & RemoveLabel methods manages the Labels collection
         * in a safe way (prevents adding the same label twice) */

        public virtual Issue AddLabel(Guid labelId)
        {
            if (Labels.Any(l => l.LabelId == labelId))
            {
                return this;
            }
            Labels.Add(new IssueLabel(Id, labelId));
            return this;
        }

        public virtual Issue RemoveLabel(Guid labelId)
        {
            Labels.RemoveAll(l => l.LabelId == labelId);
            return this;
        }

        /* Close & ReOpen methods protect the consistency
         * of the IsClosed and the CloseReason properties. */

        public virtual void Close(IssueCloseReason reason)
        {
            IsClosed = true;
            CloseReason = reason;
        }

        public virtual void ReOpen()
        {
            IsClosed = false;
            CloseReason = null;
        }
    }

    public interface IssueRepository : IBasicRepository<Issue, Guid>
    {
        //...
    }

    public class IssueManager : DomainService
    {
        //...
    }

}
