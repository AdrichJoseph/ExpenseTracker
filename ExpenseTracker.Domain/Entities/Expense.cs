using ExpenseTracker.Domain.Common;
using ExpenseTracker.Domain.Enums;
using ExpenseTracker.Domain.Exceptions;

namespace ExpenseTracker.Domain.Entities;

public class Expense : Entity
{
    // ── DATA ──────────────────────────────────────────────────────────

    public required Guid UserId { get; set; }

    public required Guid CategoryId { get; set; }
    public Category? Category { get; set; }

    public decimal Amount { get; set; }

    // 3-letter ISO code: USD, CAD, EUR.
    // Not an enum because we don't want to recompile to add a new currency.
    public string Currency { get; set; } = "CAD";

    public string Description { get; set; } = string.Empty;
    public DateTime ExpenseDate { get; set; }

    // The blob URL for the uploaded receipt (Phase 9).
    public string? ReceiptBlobUrl { get; set; }

    // ── STATE ─────────────────────────────────────────────────────────
    // The "set" is private — only domain methods on this class can change status.
    // This is what makes it a STATE MACHINE: outside callers can't just go
    //   expense.Status = ExpenseStatus.Approved;
    // they must call expense.Approve(...).
    public ExpenseStatus Status { get; private set; } = ExpenseStatus.Draft;

    public DateTime? SubmittedDate { get; private set; }
    public Guid? ApproverId { get; private set; }
    public DateTime? ApprovedDate { get; private set; }
    public string? RejectionReason { get; private set; }

    // Audit trail — every state transition pushes an entry here.
    public ICollection<ApprovalHistory> ApprovalHistory { get; private set; }
        = new List<ApprovalHistory>();

    // ── DOMAIN METHODS (the state machine) ────────────────────────────
    //
    // The pattern: each method represents a business action. It validates
    // the current state, mutates the entity, and adds an audit entry.
    // Throws DomainException if the transition is illegal.

    public void Submit()
    {
        // GUARD: only Drafts can be submitted.
        if (Status != ExpenseStatus.Draft)
            throw new DomainException(
                $"Cannot submit an expense in {Status} state. Only Draft expenses can be submitted.");

        if (Amount <= 0)
            throw new DomainException("Cannot submit an expense with non-positive amount.");

        if (string.IsNullOrWhiteSpace(Description))
            throw new DomainException("Cannot submit an expense without a description.");

        // STATE TRANSITION:
        Status = ExpenseStatus.Submitted;
        SubmittedDate = DateTime.UtcNow;
        MarkUpdated();

        // AUDIT TRAIL:
        ApprovalHistory.Add(new ApprovalHistory
        {
            ExpenseId = Id,
            ActorId = UserId,
            Action = ApprovalAction.Submitted,
            Comment = "Expense submitted for approval"
        });
    }

    public void Approve(Guid approverId, string? comment = null)
    {
        if (Status != ExpenseStatus.Submitted)
            throw new DomainException(
                $"Cannot approve an expense in {Status} state. Only Submitted expenses can be approved.");

        if (approverId == UserId)
            throw new DomainException("Users cannot approve their own expenses.");

        Status = ExpenseStatus.Approved;
        ApproverId = approverId;
        ApprovedDate = DateTime.UtcNow;
        MarkUpdated();

        ApprovalHistory.Add(new ApprovalHistory
        {
            ExpenseId = Id,
            ActorId = approverId,
            Action = ApprovalAction.Approved,
            Comment = comment ?? "Approved"
        });
    }

    public void Reject(Guid approverId, string reason)
    {
        if (Status != ExpenseStatus.Submitted)
            throw new DomainException(
                $"Cannot reject an expense in {Status} state. Only Submitted expenses can be rejected.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("A rejection requires a reason.");

        if (approverId == UserId)
            throw new DomainException("Users cannot reject their own expenses.");

        Status = ExpenseStatus.Rejected;
        ApproverId = approverId;
        RejectionReason = reason;
        MarkUpdated();

        ApprovalHistory.Add(new ApprovalHistory
        {
            ExpenseId = Id,
            ActorId = approverId,
            Action = ApprovalAction.Rejected,
            Comment = reason
        });
    }

    public void MarkReimbursed(Guid actorId)
    {
        if (Status != ExpenseStatus.Approved)
            throw new DomainException(
                $"Cannot reimburse an expense in {Status} state. Only Approved expenses can be reimbursed.");

        Status = ExpenseStatus.Reimbursed;
        MarkUpdated();

        ApprovalHistory.Add(new ApprovalHistory
        {
            ExpenseId = Id,
            ActorId = actorId,
            Action = ApprovalAction.Reimbursed,
            Comment = "Marked as reimbursed"
        });
    }
}