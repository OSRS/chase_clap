using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace OsrsOpen.Chase.Reference
{
    public delegate string KeyExtractor(ProcessingEntity entity);

    /// <summary>
    /// The data type which serves as a container for all flows.
    /// A processing entity is a data time that flows through the system.
    /// </summary>
    public abstract class ProcessingEntity
    {
        public Guid EntityTypeId
        {
            [DebuggerStepThrough]
            get;
        }

        private readonly Dictionary<string, object?> properties = new Dictionary<string, object?>();

        public object? this[string key]
        {
            [DebuggerStepThrough]
            get
            {
                if (key!=null && properties.ContainsKey(key))
                    return properties[key];
                return null;
            }

            [DebuggerStepThrough]
            set
            {
                if (key!=null)
                {
                    properties[key] = value;
                }
            }
        }

        public T? GetAs<T>(string key) where T:class
        {
            if (key!=null && properties.ContainsKey(key))
            {
                return properties[key] as T;
            }
            return null;
        }


        [DebuggerStepThrough]
        protected ProcessingEntity(Guid entityTypeId)
        {
            this.EntityTypeId = entityTypeId;
        }
    }

    public abstract class ProcessingEntityInstant : ProcessingEntity
    {
        public DateTime Timestamp
        {
            [DebuggerStepThrough]
            get;
        }

        [DebuggerStepThrough]
        protected ProcessingEntityInstant(Guid entityTypeId, DateTime timestamp) : base(entityTypeId)
        {
            this.Timestamp = timestamp;
        }
    }

    public abstract class ProcessingEntityRange : ProcessingEntity
    {
        public TimeRange TimeRange
        {
            [DebuggerStepThrough]
            get;
        }

        [DebuggerStepThrough]
        protected ProcessingEntityRange(Guid entityTypeId, TimeRange timerange) : base(entityTypeId)
        {
            if (timerange == null)
                throw new ArgumentNullException();

            this.TimeRange = timerange;
        }
    }
}
