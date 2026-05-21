namespace ExpenseTracker.Application.Approvals.Dtos;

/// <summary>
/// Request body for an approval action (approve or reject).
/// For approve, Comment is optional. For reject, it carries the required reason.
/// </summary>
public record ApprovalActionRequest(string? Comment);