using System;
using System.Collections;
using System.Collections.Generic;
using Sentry;
using UnityEngine;

public class BugReportService
{
	public static BugReportService Instance = new BugReportService();

	private const float MinSecondsBetweenUserFeedbackSubmissions = 20f;

	private const string IssueTypeSentryTag = "among_us_issue_type";

	public static readonly StringNames[] BugReportCategories = new StringNames[5]
	{
		StringNames.BugReportCategoryGeneral,
		StringNames.BugReportCategoryGameplayIssue,
		StringNames.BugReportCategoryAccountManagement,
		StringNames.BugReportCategoryServerIssues,
		StringNames.BugReportCategoryBilling
	};

	private float timeOfLastSentUserFeedback = -20f;

	private BugReportService()
	{
	}

	public bool CanSubmitFeedback()
	{
		if (Time.time - timeOfLastSentUserFeedback < 20f)
		{
			return false;
		}
		return true;
	}

	public IEnumerator SubmitUserFeedbackWithScreenshot(StringNames bugReportCategory, string email, string comments)
	{
		if (!CanSubmitFeedback())
		{
			Error("Attempted to submit user feedback too quickly", "BugReportService");
			yield break;
		}
		yield return (object)new WaitForEndOfFrame();
		Texture2D texture = ScreenCapture.CaptureScreenshotAsTexture();
		string categoryString = bugReportCategory.ToString();
		SentryId eventId = SentrySdk.CaptureMessage(categoryString, (Action<Scope>)((Scope scope) =>
		{
			ScopeExtensions.AddAttachment(scope, ImageConversion.EncodeToJPG(texture), "UserFeedbackScreenshot.png", (AttachmentType)0, (string)null);
			scope.SetTag("among_us_issue_type", categoryString);
		}), (SentryLevel)1);
		SendFeedback(email, comments, eventId);
	}

	public void SubmitUserFeedback(StringNames bugReportCategory, string email, string comments)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (!CanSubmitFeedback())
		{
			Error("Attempted to submit user feedback too quickly", "BugReportService");
			return;
		}
		string categoryString = bugReportCategory.ToString();
		SentryId eventId = SentrySdk.CaptureMessage(categoryString, (Action<Scope>)((Scope scope) =>
		{
			scope.SetTag("among_us_issue_type", categoryString);
		}), (SentryLevel)1);
		SendFeedback(email, comments, eventId);
	}

	public void Info(string message, string category)
	{
		SentrySdk.AddBreadcrumb(message, category, (string)null, (IDictionary<string, string>)null, (BreadcrumbLevel)0);
	}

	public void Debug(string message, string category)
	{
		SentrySdk.AddBreadcrumb(message, category, (string)null, (IDictionary<string, string>)null, (BreadcrumbLevel)(-1));
	}

	public void Warning(string message, string category)
	{
		SentrySdk.AddBreadcrumb(message, category, (string)null, (IDictionary<string, string>)null, (BreadcrumbLevel)1);
	}

	public void Error(string message, string category)
	{
		SentrySdk.AddBreadcrumb(message, category, (string)null, (IDictionary<string, string>)null, (BreadcrumbLevel)2);
	}

	public void Exception(Exception exception)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		SentrySdk.CaptureException(exception);
	}

	private void SendFeedback(string email, string comments, SentryId eventId)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		SentrySdk.CaptureUserFeedback(eventId, email, comments, (string)null);
		timeOfLastSentUserFeedback = Time.time;
	}
}
