namespace System
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    internal class MonoDocumentationNoteAttribute : MonoTODOAttribute
    {
        public MonoDocumentationNoteAttribute(string comment)
            : base(comment)
        {
        }

        public override string Comment
        {
            get { return base.Comment; }
        }
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    internal class MonoExtensionAttribute : MonoTODOAttribute
    {
        public MonoExtensionAttribute(string comment)
            : base(comment)
        {
        }

        public override string Comment
        {
            get { return base.Comment; }
        }
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    internal class MonoInternalNoteAttribute : MonoTODOAttribute
    {
        public MonoInternalNoteAttribute(string comment)
            : base(comment)
        {
        }

        public override string Comment
        {
            get { return base.Comment; }
        }
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    internal class MonoLimitationAttribute : MonoTODOAttribute
    {
        public MonoLimitationAttribute(string comment)
            : base(comment)
        {
        }

        public override string Comment
        {
            get { return base.Comment; }
        }
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    internal class MonoNotSupportedAttribute : MonoTODOAttribute
    {
        public MonoNotSupportedAttribute(string comment)
            : base(comment)
        {
        }

        public override string Comment
        {
            get { return base.Comment; }
        }
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    internal class MonoTODOAttribute : Attribute
    {
        private string comment;

        public MonoTODOAttribute()
        {
        }

        public MonoTODOAttribute(string comment)
        {
            this.comment = comment;
        }

        public virtual string Comment
        {
            get { return comment; }
        }
    }
}
