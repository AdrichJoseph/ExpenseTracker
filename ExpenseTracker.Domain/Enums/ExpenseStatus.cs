namespace ExpenseTracker.Domain.Enums;

// The lifecycle of an expense:
//
//   Draft ──Submit──> Submitted ──Approve──> Approved ──Reimburse──> Reimbursed
//                          │
//                          └──Reject──> Rejected
//
// Plus from any state, an admin can force-cancel (we'll allow that via a domain method).
public enum ExpenseStatus
{
    Draft = 1,
    Submitted = 2,
    Approved = 3,
    Rejected = 4,
    Reimbursed = 5
}