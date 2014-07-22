using System;

namespace Reference.Model
{
    public class HtmlModel
    {
        public virtual String UserId { get; set; }
        public virtual int HtmlId { get; set; }
        public virtual String URL { get; set; }
        public virtual DateTime CreateTime { get; set; }

        public override bool Equals(object obj)
        {
            HtmlModel model = obj as HtmlModel;

            return ((this.UserId == model.UserId)
                && (this.HtmlId == model.HtmlId));
        }

        public override int GetHashCode()
        {
            return String.Format("%s|%d", UserId, HtmlId).GetHashCode();
        }
    }
}
