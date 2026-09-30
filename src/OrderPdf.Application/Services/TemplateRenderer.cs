using System.Net;
using System.Text;
using OrderPdf.Application.Interfaces;
using OrderPdf.Application.Models;

namespace OrderPdf.Application.Services;

public sealed class TemplateRenderer : ITemplateRenderer
{
    public Task<string> RenderOrderConfirmationHtmlAsync(OrderPdfViewModel model, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sb = new StringBuilder();

        sb.Append($$"""
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Order Confirmation - {{WebUtility.HtmlEncode(model.OrderNumber)}}</title>
    <style>
        * {
            box-sizing: border-box;
            margin: 0;
            padding: 0;
        }
        body {
            font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif;
            font-size: 12px;
            color: #1e293b;
            background-color: #ffffff;
            line-height: 1.5;
            padding: 24px 32px;
        }

        /* Header Section */
        .header-container {
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            border-bottom: 2px solid #e2e8f0;
            padding-bottom: 20px;
            margin-bottom: 24px;
        }
        .company-brand h1 {
            font-size: 22px;
            font-weight: 800;
            color: #0f172a;
            letter-spacing: -0.5px;
            margin-bottom: 4px;
        }
        .company-tagline {
            font-size: 11px;
            color: #64748b;
            font-weight: 500;
            margin-bottom: 8px;
        }
        .company-contact {
            font-size: 11px;
            color: #475569;
            line-height: 1.4;
        }
        .order-title-badge {
            text-align: right;
        }
        .document-title {
            font-size: 20px;
            font-weight: 800;
            color: #2563eb;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }
        .order-meta-box {
            margin-top: 8px;
            background: #f8fafc;
            border: 1px solid #e2e8f0;
            border-radius: 6px;
            padding: 10px 14px;
            text-align: left;
            min-width: 220px;
        }
        .order-meta-row {
            display: flex;
            justify-content: space-between;
            margin-bottom: 4px;
            font-size: 11px;
        }
        .order-meta-row:last-child {
            margin-bottom: 0;
        }
        .order-meta-label {
            font-weight: 600;
            color: #64748b;
        }
        .order-meta-value {
            font-weight: 700;
            color: #0f172a;
        }

        /* Customer & Shipping 2-Column Section */
        .parties-container {
            display: flex;
            justify-content: space-between;
            gap: 20px;
            margin-bottom: 24px;
        }
        .party-card {
            flex: 1;
            background: #f8fafc;
            border: 1px solid #e2e8f0;
            border-radius: 6px;
            padding: 14px 16px;
        }
        .party-title {
            font-size: 11px;
            font-weight: 700;
            color: #2563eb;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            margin-bottom: 8px;
            border-bottom: 1px solid #cbd5e1;
            padding-bottom: 4px;
        }
        .party-name {
            font-size: 13px;
            font-weight: 700;
            color: #0f172a;
            margin-bottom: 2px;
        }
        .party-details {
            font-size: 11px;
            color: #475569;
            line-height: 1.4;
        }

        /* Order Items Table */
        .items-section {
            margin-bottom: 24px;
        }
        .items-table {
            width: 100%;
            border-collapse: collapse;
            font-size: 11px;
        }
        .items-table thead {
            display: table-header-group;
        }
        .items-table th {
            background-color: #0f172a;
            color: #ffffff;
            font-weight: 600;
            text-align: left;
            padding: 8px 10px;
            text-transform: uppercase;
            font-size: 10px;
            letter-spacing: 0.5px;
        }
        .items-table th.num-col, .items-table td.num-col {
            text-align: right;
        }
        .items-table th.center-col, .items-table td.center-col {
            text-align: center;
        }
        .items-table tbody tr {
            page-break-inside: avoid;
            border-bottom: 1px solid #e2e8f0;
        }
        .items-table tbody tr:nth-child(even) {
            background-color: #f8fafc;
        }
        .items-table td {
            padding: 8px 10px;
            vertical-align: middle;
            color: #334155;
        }
        .item-sku {
            font-family: monospace;
            font-weight: 600;
            color: #475569;
            font-size: 10px;
        }
        .item-name {
            font-weight: 600;
            color: #0f172a;
        }
        .item-desc {
            font-size: 10px;
            color: #64748b;
        }

        /* Summary & Totals */
        .summary-container {
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            page-break-inside: avoid;
            gap: 20px;
            margin-top: 16px;
        }
        .notes-card {
            flex: 1.2;
            background: #ffffff;
            border: 1px dashed #cbd5e1;
            border-radius: 6px;
            padding: 12px 16px;
        }
        .notes-title {
            font-size: 11px;
            font-weight: 700;
            color: #334155;
            margin-bottom: 6px;
        }
        .notes-content {
            font-size: 11px;
            color: #64748b;
            line-height: 1.4;
        }
        .totals-card {
            flex: 0.8;
            background: #f8fafc;
            border: 1px solid #cbd5e1;
            border-radius: 6px;
            padding: 14px 18px;
            min-width: 250px;
        }
        .totals-row {
            display: flex;
            justify-content: space-between;
            margin-bottom: 6px;
            font-size: 11px;
            color: #475569;
        }
        .totals-row.discount {
            color: #16a34a;
        }
        .totals-row.grand-total {
            border-top: 2px solid #0f172a;
            padding-top: 8px;
            margin-top: 8px;
            font-size: 14px;
            font-weight: 800;
            color: #0f172a;
        }

        /* Print Specifics */
        @media print {
            body {
                padding: 0;
            }
            .items-table thead {
                display: table-header-group;
            }
            .items-table tbody tr {
                page-break-inside: avoid;
            }
            .summary-container {
                page-break-inside: avoid;
            }
        }
    </style>
</head>
<body>

    <!-- Header & Company Branding -->
    <div class="header-container">
        <div class="company-brand">
            <h1>{{WebUtility.HtmlEncode(model.Company.Name)}}</h1>
            <div class="company-tagline">{{WebUtility.HtmlEncode(model.Company.Tagline)}}</div>
            <div class="company-contact">
                <div>{{WebUtility.HtmlEncode(model.Company.Address.Street)}}</div>
                <div>{{WebUtility.HtmlEncode(model.Company.Address.FormattedCityStateZip)}}, {{WebUtility.HtmlEncode(model.Company.Address.Country)}}</div>
                <div>Email: {{WebUtility.HtmlEncode(model.Company.Email)}} | Phone: {{WebUtility.HtmlEncode(model.Company.Phone)}}</div>
                <div>Web: {{WebUtility.HtmlEncode(model.Company.Website)}} | Tax ID: {{WebUtility.HtmlEncode(model.Company.TaxId)}}</div>
            </div>
        </div>
        <div class="order-title-badge">
            <div class="document-title">Order Confirmation</div>
            <div class="order-meta-box">
                <div class="order-meta-row">
                    <span class="order-meta-label">Order Number:</span>
                    <span class="order-meta-value">{{WebUtility.HtmlEncode(model.OrderNumber)}}</span>
                </div>
                <div class="order-meta-row">
                    <span class="order-meta-label">Order Date:</span>
                    <span class="order-meta-value">{{WebUtility.HtmlEncode(model.OrderDateFormatted)}}</span>
                </div>
                <div class="order-meta-row">
                    <span class="order-meta-label">Status:</span>
                    <span class="order-meta-value">{{WebUtility.HtmlEncode(model.Status)}}</span>
                </div>
                <div class="order-meta-row">
                    <span class="order-meta-label">Currency:</span>
                    <span class="order-meta-value">{{WebUtility.HtmlEncode(model.Currency)}}</span>
                </div>
            </div>
        </div>
    </div>

    <!-- Customer Billing & Shipping Info -->
    <div class="parties-container">
        <div class="party-card">
            <div class="party-title">Bill To</div>
            <div class="party-name">{{WebUtility.HtmlEncode(model.CustomerCompanyName)}}</div>
            <div class="party-details">
                <div>Attn: {{WebUtility.HtmlEncode(model.CustomerName)}}</div>
                <div>{{WebUtility.HtmlEncode(model.BillingAddress.Street)}}</div>
                <div>{{WebUtility.HtmlEncode(model.BillingAddress.FormattedCityStateZip)}}</div>
                <div>{{WebUtility.HtmlEncode(model.BillingAddress.Country)}}</div>
                <div>Email: {{WebUtility.HtmlEncode(model.CustomerEmail)}}</div>
                <div>Phone: {{WebUtility.HtmlEncode(model.CustomerPhone)}}</div>
            </div>
        </div>
        <div class="party-card">
            <div class="party-title">Ship To</div>
            <div class="party-name">{{WebUtility.HtmlEncode(model.CustomerCompanyName)}}</div>
            <div class="party-details">
                <div>Attn: {{WebUtility.HtmlEncode(model.CustomerName)}}</div>
                <div>{{WebUtility.HtmlEncode(model.ShippingAddress.Street)}}</div>
                <div>{{WebUtility.HtmlEncode(model.ShippingAddress.FormattedCityStateZip)}}</div>
                <div>{{WebUtility.HtmlEncode(model.ShippingAddress.Country)}}</div>
                <div>Payment: {{WebUtility.HtmlEncode(model.PaymentMethod)}}</div>
            </div>
        </div>
    </div>

    <!-- Line Items Table -->
    <div class="items-section">
        <table class="items-table">
            <thead>
                <tr>
                    <th class="center-col" style="width: 30px;">#</th>
                    <th style="width: 85px;">SKU</th>
                    <th style="width: 140px;">Product</th>
                    <th>Description</th>
                    <th class="center-col" style="width: 40px;">Qty</th>
                    <th class="num-col" style="width: 75px;">Unit Price</th>
                    <th class="num-col" style="width: 65px;">Discount</th>
                    <th class="num-col" style="width: 80px;">Tax</th>
                    <th class="num-col" style="width: 85px;">Total</th>
                </tr>
            </thead>
            <tbody>
""");

        foreach (var item in model.Items)
        {
            sb.Append($$"""
                <tr>
                    <td class="center-col">{{item.Index}}</td>
                    <td class="item-sku">{{WebUtility.HtmlEncode(item.SKU)}}</td>
                    <td class="item-name">{{WebUtility.HtmlEncode(item.ProductName)}}</td>
                    <td class="item-desc">{{WebUtility.HtmlEncode(item.Description)}}</td>
                    <td class="center-col">{{item.Quantity}}</td>
                    <td class="num-col">{{item.FormattedUnitPrice}}</td>
                    <td class="num-col">{{item.FormattedDiscount}}</td>
                    <td class="num-col">{{item.FormattedTax}}</td>
                    <td class="num-col" style="font-weight: 600;">{{item.FormattedLineTotal}}</td>
                </tr>
""");
        }

        sb.Append($$"""
            </tbody>
        </table>
    </div>

    <!-- Financial Summary & Notes -->
    <div class="summary-container">
        <div class="notes-card">
            <div class="notes-title">Notes & Customer Instructions</div>
            <div class="notes-content">
                {{(string.IsNullOrWhiteSpace(model.CustomerNotes) ? "Thank you for choosing Acme Commerce Solutions. Please reference your order number for any inquiries regarding delivery tracking or invoicing." : WebUtility.HtmlEncode(model.CustomerNotes))}}
            </div>
        </div>
        <div class="totals-card">
            <div class="totals-row">
                <span>Subtotal ({{model.TotalItemCount}} items):</span>
                <span>{{model.FormattedSubtotal}}</span>
            </div>
""");

        if (model.TotalDiscount > 0)
        {
            sb.Append($$"""
            <div class="totals-row discount">
                <span>Total Savings / Discounts:</span>
                <span>{{model.FormattedDiscount}}</span>
            </div>
""");
        }

        sb.Append($$"""
            <div class="totals-row">
                <span>Estimated Tax:</span>
                <span>{{model.FormattedTax}}</span>
            </div>
            <div class="totals-row">
                <span>Shipping & Handling:</span>
                <span>{{model.FormattedShipping}}</span>
            </div>
            <div class="totals-row grand-total">
                <span>Grand Total:</span>
                <span>{{model.FormattedGrandTotal}}</span>
            </div>
        </div>
    </div>

</body>
</html>
""");

        return Task.FromResult(sb.ToString());
    }
}
