namespace ExpenseTracker.Domain.Enums;

// Used in the audit trail: every entry in ApprovalHistory records WHICH action
// was taken. Reports can answer "show me everyone's rejections last quarter".
public enum ApprovalAction
{
    Submitted = 1,
    Approved = 2,
    Rejected = 3,
    Reimbursed = 4,
    Returned = 5  // future-proofing: manager wants more info before approving
}