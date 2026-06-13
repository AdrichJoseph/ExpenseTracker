using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using ExpenseTracker.Domain.Exceptions;

namespace ExpenseTracker.UnitTests;

// ════════════════════════════════════════════════════════════════════════════
// ExpenseStateTests
//
// These tests cover the Expense entity's state machine: the four domain
// methods Submit(), Approve(), Reject(), and MarkReimbursed().
//
// WHY NO MOCKS HERE?
// Expense is a plain C# object. It has no constructor parameters, no
// database, no HTTP client — nothing to fake out. We just create one with
// `new Expense { ... }`, call a method, and check what happened. That's the
// definition of a pure unit test: isolated, fast, no infrastructure.
// ════════════════════════════════════════════════════════════════════════════

public class ExpenseStateTests
{
    // ── Submit ───────────────────────────────────────────────────────────────

    [Fact]
    public void Submit_ValidDraft_SetsStatusToSubmitted()
    {
        // Arrange: build a fresh Draft expense using the helper at the bottom.
        // "Arrange" means "set the world up so we can test one specific thing."
        var expense = CreateDraftExpense();

        // Act: call the one method we're testing.
        // "Act" is always a single line — if you're doing more here, the test
        // is probably testing two things at once.
        expense.Submit();

        // Assert: check the outcome.
        // Assert.Equal(expected, actual) — note the order: expected first, actual second.
        // xUnit uses this order everywhere; if you swap them the failure message
        // reads backwards ("expected Submitted but got Draft" vs the reverse).
        Assert.Equal(ExpenseStatus.Submitted, expense.Status);
    }

    [Fact]
    public void Submit_ValidDraft_SetsSubmittedDate()
    {
        // We capture "now" before the act so we can assert the date was set
        // to approximately now (not null, not some ancient value).
        var expense = CreateDraftExpense();
        var before = DateTime.UtcNow;

        expense.Submit();

        // Assert.NotNull confirms the property changed from null to a real value.
        Assert.NotNull(expense.SubmittedDate);
        // The submitted date should be >= the moment we measured before calling Submit.
        Assert.True(expense.SubmittedDate >= before);
    }

    [Fact]
    public void Submit_WhenAlreadySubmitted_ThrowsDomainException()
    {
        // Arrange: put the expense into Submitted state first.
        // We do this by calling Submit() once — that's fine inside Arrange.
        var expense = CreateDraftExpense();
        expense.Submit();

        // Act & Assert combined: Assert.Throws<T>(() => ...) asserts that
        // running the lambda throws exactly T. If Submit() returns normally
        // instead of throwing, xUnit fails the test with a clear message.
        Assert.Throws<DomainException>(() => expense.Submit());
    }

    [Fact]
    public void Submit_WhenAmountIsZero_ThrowsDomainException()
    {
        // Amount = 0 is the exact boundary the domain guards against (Amount <= 0).
        // Boundary-value tests like this are high-value: bugs often live exactly
        // at the edge of a condition, not deep in the middle.
        var expense = CreateDraftExpense(amount: 0m);

        Assert.Throws<DomainException>(() => expense.Submit());
    }

    [Fact]
    public void Submit_WhenAmountIsNegative_ThrowsDomainException()
    {
        // -1 is "clearly over the boundary" — a complement to the zero test above.
        // Together they document that the rule is Amount <= 0, not just Amount == 0.
        var expense = CreateDraftExpense(amount: -1m);

        Assert.Throws<DomainException>(() => expense.Submit());
    }

    [Fact]
    public void Submit_WhenDescriptionIsEmpty_ThrowsDomainException()
    {
        var expense = CreateDraftExpense(description: "");

        Assert.Throws<DomainException>(() => expense.Submit());
    }

    // ── Approve ──────────────────────────────────────────────────────────────

    [Fact]
    public void Approve_WhenSubmitted_SetsStatusToApproved()
    {
        // To test Approve, we first need the expense in Submitted state.
        // Using Submit() here is intentional: we're relying on a domain method
        // we already tested above. If Submit() itself were broken, the Submit tests
        // above would catch it — not this test.
        var expense = CreateDraftExpense();
        expense.Submit();

        var approverId = Guid.NewGuid(); // different from the expense's UserId

        expense.Approve(approverId);

        Assert.Equal(ExpenseStatus.Approved, expense.Status);
        Assert.Equal(approverId, expense.ApproverId);
        Assert.NotNull(expense.ApprovedDate);
    }

    [Fact]
    public void Approve_WhenDraft_ThrowsDomainException()
    {
        // A brand-new expense is a Draft. Approving a Draft is illegal.
        var expense = CreateDraftExpense();

        Assert.Throws<DomainException>(() => expense.Approve(Guid.NewGuid()));
    }

    [Fact]
    public void Approve_BySameUser_ThrowsDomainException()
    {
        // The domain rule: you cannot approve your own expense.
        // We use a named variable for the userId so we can pass the same Guid
        // to both the expense and the Approve() call — making the test intention clear.
        var userId = Guid.NewGuid();
        var expense = CreateDraftExpense(userId: userId);
        expense.Submit();

        // Passing the same userId as the approverId should throw.
        Assert.Throws<DomainException>(() => expense.Approve(userId));
    }

    // ── Reject ───────────────────────────────────────────────────────────────

    [Fact]
    public void Reject_WhenSubmitted_SetsStatusToRejected()
    {
        var expense = CreateDraftExpense();
        expense.Submit();
        var approverId = Guid.NewGuid();

        expense.Reject(approverId, "Duplicate expense — already reimbursed last week.");

        Assert.Equal(ExpenseStatus.Rejected, expense.Status);
        Assert.Equal("Duplicate expense — already reimbursed last week.", expense.RejectionReason);
    }

    [Fact]
    public void Reject_WhenDraft_ThrowsDomainException()
    {
        var expense = CreateDraftExpense();

        Assert.Throws<DomainException>(() => expense.Reject(Guid.NewGuid(), "some reason"));
    }

    [Fact]
    public void Reject_WhenReasonIsEmpty_ThrowsDomainException()
    {
        // Even in Submitted state, an empty reason is illegal.
        var expense = CreateDraftExpense();
        expense.Submit();

        Assert.Throws<DomainException>(() => expense.Reject(Guid.NewGuid(), ""));
    }

    // ── MarkReimbursed ───────────────────────────────────────────────────────

    [Fact]
    public void MarkReimbursed_WhenApproved_SetsStatusToReimbursed()
    {
        // MarkReimbursed requires Approved state, so we walk through the full
        // happy-path chain: Draft → Submitted → Approved → Reimbursed.
        var expense = CreateDraftExpense();
        var approverId = Guid.NewGuid();
        expense.Submit();
        expense.Approve(approverId);

        expense.MarkReimbursed(approverId);

        Assert.Equal(ExpenseStatus.Reimbursed, expense.Status);
    }

    [Fact]
    public void MarkReimbursed_WhenNotApproved_ThrowsDomainException()
    {
        // A Draft expense cannot be reimbursed — must go through the full workflow.
        var expense = CreateDraftExpense();

        Assert.Throws<DomainException>(() => expense.MarkReimbursed(Guid.NewGuid()));
    }

    // ── Helper ───────────────────────────────────────────────────────────────
    //
    // A factory method that produces a valid Draft expense.
    // Named parameters (amount:, description:, userId:) let individual tests
    // override just the one field they care about — everything else gets a
    // sensible default. This keeps test bodies focused on the variable under test.

    private static Expense CreateDraftExpense(
        decimal amount = 100m,
        string description = "Taxi to client site",
        Guid? userId = null)
    {
        return new Expense
        {
            UserId = userId ?? Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Amount = amount,
            Currency = "CAD",
            Description = description,
            ExpenseDate = DateTime.UtcNow.AddDays(-1)
        };
    }
}
