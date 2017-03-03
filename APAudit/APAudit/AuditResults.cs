namespace APAudit
{
    public enum AuditCode { OK, ExtraKey, MissingKey, WrongValue}

    public class AuditResults
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string DesiredValue { get; set; }
        public AuditCode AuditCode { get; set; }
    }
}