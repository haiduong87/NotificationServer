using System;
using System.Text.Json.Serialization;

namespace NotificationServer.Dto
{
    public class DatabaseNotification
    {
        private static readonly string PreSerializedTemplate =
            @"{{""id"":{0},""table"":""{1}"",""action"":""{2}"",""customer"":""{3}"",""{4}"":""{5:yyyy-dd-MMThh:mm:ss.fffZ}""}}";

        private static readonly string Insert = "insert";
        private static readonly string Update = "update";
        private static readonly string Delete = "delete";
        private static readonly string Now = "now";

        private static readonly string createdDate = nameof(createdDate);
        private static readonly string modifiedDate = nameof(modifiedDate);
        private static readonly string deletedDate = nameof(deletedDate);

        private (string Action, string DateField, DateTime DateValue)? _notificationTuple;

        private (string Action, string DateField, DateTime DateValue) NotificationTuple => _notificationTuple ??=
            CreatedDate.HasValue ? (Insert, createdDate, CreatedDate.Value) :
            ModifiedDate.HasValue ? (Update, modifiedDate, ModifiedDate.Value) :
            DeletedDate.HasValue ? (Delete, deletedDate, DeletedDate.Value) : (string.Empty, Now, DateTime.Now);

        [JsonPropertyName("id")] public long? Id { get; set; }

        [JsonPropertyName("table")] public string Table { get; set; }

        [JsonPropertyName("action")] public string Action => NotificationTuple.Action;

        [JsonPropertyName("customer")] public string Customer { get; set; }

        [JsonPropertyName("createdDate")] public DateTime? CreatedDate { get; set; }

        [JsonPropertyName("modifiedDate")] public DateTime? ModifiedDate { get; set; }

        [JsonPropertyName("deletedDate")] public DateTime? DeletedDate { get; set; }

        public string SerializedString =>
            string.Format(PreSerializedTemplate, Id, Table, Action, Customer, NotificationTuple.DateField,
                NotificationTuple.DateValue);
    }
}