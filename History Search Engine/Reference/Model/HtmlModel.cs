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

        public override string ToString()
        {
            return String.Format("{0}|{1}|{2}|{3}", UserId, HtmlId, URL, CreateTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }
}
