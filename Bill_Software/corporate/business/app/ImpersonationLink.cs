using System;

namespace Bill_Software.corporate.business.app
{
    /// <summary>
    /// Session-scoped actor/target snapshot for one impersonation lease.
    /// Stored only after a successful Start. Absent while SwitchUser is false.
    /// </summary>
    [Serializable]
    public sealed class ImpersonationLink
    {
        public Guid ImpersonationId { get; set; }
        public int CompanyID { get; set; }

        public int ActorUserId { get; set; }
        public string ActorUserKey { get; set; }
        public int? ActorRoleId { get; set; }
        public string ActorRoleName { get; set; }
        public string ActorUserType { get; set; }
        public string ActorProfilePic { get; set; }
        public Guid ActorSessionToken { get; set; }

        public int TargetUserId { get; set; }
        public string TargetUserKey { get; set; }
        public int? TargetRoleId { get; set; }
        public string TargetRoleName { get; set; }
        public string TargetProfilePic { get; set; }
        public Guid TargetSessionToken { get; set; }
    }
}
