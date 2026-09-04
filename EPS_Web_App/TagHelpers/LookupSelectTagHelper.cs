using EPS_Web_App.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;
using System.Text.Encodings.Web;

namespace EPS_Web_App.TagHelpers;

[HtmlTargetElement(
	"select",
	Attributes = "asp-for"
)]
public sealed class LookupSelectTagHelper : TagHelper
{
	private readonly LookupCatalog _lookupCatalog;

	public LookupSelectTagHelper(
		LookupCatalog lookupCatalog)
	{
		_lookupCatalog =
			lookupCatalog;
	}


	// =========================================================
	// RUN AFTER BUILT-IN SELECT TAG HELPER
	// =========================================================

	public override int Order =>
		int.MaxValue;


	[HtmlAttributeName("asp-for")]
	public ModelExpression For { get; set; } =
		default!;


	[ViewContext]
	public ViewContext ViewContext { get; set; } =
		default!;


	public override async Task ProcessAsync(
		TagHelperContext context,
		TagHelperOutput output)
	{
		var propertyName =
			GetPropertyName(
				For.Name
			);


		if (string.IsNullOrWhiteSpace(
				propertyName))
		{
			return;
		}


		var group =
			LookupCatalog.GetGroupForProperty(
				propertyName
			);


		if (string.IsNullOrWhiteSpace(
				group))
		{
			return;
		}


		// -----------------------------------------------------
		// Get active lookup values.
		// -----------------------------------------------------

		var options =
			await _lookupCatalog
				.GetActiveOptionsAsync(
					group
				);


		// -----------------------------------------------------
		// If no lookup values exist, preserve the original
		// select options instead of blanking the control.
		// -----------------------------------------------------

		if (options.Count == 0)
		{
			return;
		}


		var currentValue =
			Convert.ToString(
				For.Model
			)
			??
			string.Empty;


		var html =
			new StringBuilder();


		// -----------------------------------------------------
		// PLACEHOLDER
		// -----------------------------------------------------

		html.Append(
			"<option value=\"\">Select</option>"
		);


		// -----------------------------------------------------
		// DATABASE OPTIONS
		// -----------------------------------------------------

		foreach (
			var option
			in options)
		{
			var selected =
				string.Equals(
					currentValue.Trim(),
					option.Code.Trim(),
					StringComparison.OrdinalIgnoreCase
				);


			html.Append(
				"<option value=\""
			);


			html.Append(
				HtmlEncoder.Default.Encode(
					option.Code
				)
			);


			html.Append(
				"\""
			);


			if (selected)
			{
				html.Append(
					" selected"
				);
			}


			html.Append(
				">"
			);


			html.Append(
				HtmlEncoder.Default.Encode(
					option.Label
				)
			);


			html.Append(
				"</option>"
			);
		}


		// -----------------------------------------------------
		// REPLACE THE ORIGINAL OPTION CONTENT
		// -----------------------------------------------------

		output.Content.SetHtmlContent(
			html.ToString()
		);


		output.TagMode =
			TagMode.StartTagAndEndTag;
	}


	// =========================================================
	// EXTRACT PROPERTY NAME
	// =========================================================

	private static string GetPropertyName(
		string? expression)
	{
		if (string.IsNullOrWhiteSpace(
				expression))
		{
			return string.Empty;
		}


		var lastDot =
			expression.LastIndexOf(
				'.'
			);


		return lastDot >= 0
			? expression[
				(lastDot + 1)..]
			: expression;
	}
}