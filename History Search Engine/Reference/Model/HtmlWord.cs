using System;

namespace Reference.Model
{
    public class HtmlWord
    {
        public virtual String UserId { get; set; }
        public virtual int HtmlId { get; set; }
        public virtual int HtmlWordId { get; set; }
        public virtual String Word { get; set; }
        public virtual int WordCount { get; set; }

        public override bool Equals(object obj)
        {
            HtmlWord model = obj as HtmlWord;

            return ((this.UserId == model.UserId)
                && (this.HtmlId == model.HtmlId)
                && (this.HtmlWordId == model.HtmlWordId));
        }

        public override int GetHashCode()
        {
            return String.Format("%s|%d|%d", UserId, HtmlId, HtmlWordId).GetHashCode();
        }
    }
}
