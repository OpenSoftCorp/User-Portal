namespace UserPortal.Models.Entities
{
    public class MenuesInfo
    {
        public string ScreenId { get; set; }
        public string ParentId { get; set; }

        public string ActionName { get; set; }
        public string ControllerName { get; set; }
        public string Icon { get; set; }
        public string hasArrow { get; set; }
        public string Flag { get; set; }
        public string ButtonRights { get; set; }
        public string Target { get; set; }

        public string SCREENNAME { get; set; }

        public string TABID { get; set; }

        public string HeadingName { get; set; }
        public string? UserName { get; internal set; }
    }
}
