using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using PayrollAPI.Models;

namespace PayrollAPI.Services
{
    public interface IPayslipPdfService
    {
        Task<byte[]> GeneratePayslipPdfAsync(
            Payslip payslip, Employee employee);
    }

    public class PayslipPdfService : IPayslipPdfService
    {
        public Task<byte[]> GeneratePayslipPdfAsync(
            Payslip payslip, Employee employee)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    // ── HEADER ──
                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            // Company name left
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("PayrollPro Ltd")
                                    .Bold().FontSize(22)
                                    .FontColor(Color.FromHex("#2563EB"));
                                c.Item().Text("HR & Payroll Management System")
                                    .FontSize(9)
                                    .FontColor(Color.FromHex("#374151"));
                            });

                            // Payslip title right
                            row.RelativeItem().AlignRight().Column(c =>
                            {
                                c.Item().AlignRight().Text("PAYSLIP")
                                    .Bold().FontSize(18)
                                    .FontColor(Color.FromHex("#2563EB"));
                                c.Item().AlignRight()
                                    .Text($"Period: {payslip.PayPeriod}")
                                    .FontSize(10)
                                    .FontColor(Color.FromHex("#374151"));
                            });
                        });

                        // Blue divider line
                        col.Item().PaddingTop(8).LineHorizontal(2)
                            .LineColor(Color.FromHex("#2563EB"));
                    });

                    // ── CONTENT ──
                    page.Content().PaddingTop(20).Column(col =>
                    {
                        // ── EMPLOYEE INFORMATION ──
                        col.Item().Text("EMPLOYEE INFORMATION")
                            .Bold().FontSize(11)
                            .FontColor(Color.FromHex("#2563EB"));

                        col.Item().PaddingTop(6).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.RelativeColumn();
                            });

                            void AddInfoRow(string label, string value)
                            {
                                table.Cell().Background(Color.FromHex("#F3F4F6"))
                                    .Padding(6)
                                    .Text(label).Bold().FontSize(9)
                                    .FontColor(Color.FromHex("#374151"));

                                table.Cell().Background(Color.FromHex("#F3F4F6"))
                                    .Padding(6)
                                    .Text(value).FontSize(9);
                            }

                            AddInfoRow("Employee Name:",
                                $"{employee.FirstName} {employee.LastName}");
                            AddInfoRow("Job Title:", employee.JobTitle);
                            AddInfoRow("Department:", employee.Department);
                            AddInfoRow("Employment Type:",
                                employee.EmploymentType);
                            AddInfoRow("Bank Name:", employee.BankName);
                            AddInfoRow("Account Number:",
                                employee.AccountNumber);
                        });

                        col.Item().PaddingTop(16);

                        // ── EARNINGS TABLE ──
                        col.Item().Text("EARNINGS")
                            .Bold().FontSize(11)
                            .FontColor(Color.FromHex("#2563EB"));

                        col.Item().PaddingTop(6).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(7);
                                c.RelativeColumn(3);
                            });

                            // Header
                            table.Cell()
                                .Background(Color.FromHex("#2563EB"))
                                .Padding(8)
                                .Text("Description").Bold().FontSize(10)
                                .FontColor(Colors.White);
                            table.Cell()
                                .Background(Color.FromHex("#2563EB"))
                                .Padding(8).AlignRight()
                                .Text("Amount (₦)").Bold().FontSize(10)
                                .FontColor(Colors.White);

                            void AddEarningRow(string desc, decimal amount,
                                bool gray = false)
                            {
                                var bg = gray ? "#F3F4F6" : "#FFFFFF";
                                table.Cell()
                                    .Background(Color.FromHex(bg))
                                    .Padding(7).Text(desc).FontSize(9);
                                table.Cell()
                                    .Background(Color.FromHex(bg))
                                    .Padding(7).AlignRight()
                                    .Text($"{amount:N2}").FontSize(9);
                            }

                            AddEarningRow("Basic Salary",
                                payslip.BasicSalary);
                            AddEarningRow("Total Allowances",
                                payslip.TotalAllowances, true);

                            // Gross total
                            table.Cell()
                                .Background(Color.FromHex("#DBEAFE"))
                                .Padding(8)
                                .Text("GROSS PAY").Bold().FontSize(10);
                            table.Cell()
                                .Background(Color.FromHex("#DBEAFE"))
                                .Padding(8).AlignRight()
                                .Text($"{payslip.GrossPay:N2}")
                                .Bold().FontSize(10);
                        });

                        col.Item().PaddingTop(16);

                        // ── DEDUCTIONS TABLE ──
                        col.Item().Text("DEDUCTIONS")
                            .Bold().FontSize(11)
                            .FontColor(Color.FromHex("#2563EB"));

                        col.Item().PaddingTop(6).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(7);
                                c.RelativeColumn(3);
                            });

                            // Header
                            table.Cell()
                                .Background(Color.FromHex("#2563EB"))
                                .Padding(8)
                                .Text("Description").Bold().FontSize(10)
                                .FontColor(Colors.White);
                            table.Cell()
                                .Background(Color.FromHex("#2563EB"))
                                .Padding(8).AlignRight()
                                .Text("Amount (₦)").Bold().FontSize(10)
                                .FontColor(Colors.White);

                            void AddDeductionRow(string desc, decimal amount,
                                bool gray = false)
                            {
                                var bg = gray ? "#F3F4F6" : "#FFFFFF";
                                table.Cell()
                                    .Background(Color.FromHex(bg))
                                    .Padding(7).Text(desc).FontSize(9);
                                table.Cell()
                                    .Background(Color.FromHex(bg))
                                    .Padding(7).AlignRight()
                                    .Text($"{amount:N2}").FontSize(9);
                            }

                            AddDeductionRow("PAYE Tax", payslip.PAYE);
                            AddDeductionRow("Employee Pension (8%)",
                                payslip.Pension, true);
                            AddDeductionRow("NHF (2.5% of Basic)",
                                payslip.NHF);
                            if (payslip.OtherDeductions > 0)
                                AddDeductionRow("Other Deductions",
                                    payslip.OtherDeductions, true);

                            // Total deductions
                            table.Cell()
                                .Background(Color.FromHex("#FEE2E2"))
                                .Padding(8)
                                .Text("TOTAL DEDUCTIONS")
                                .Bold().FontSize(10);
                            table.Cell()
                                .Background(Color.FromHex("#FEE2E2"))
                                .Padding(8).AlignRight()
                                .Text($"{payslip.TotalDeductions:N2}")
                                .Bold().FontSize(10);
                        });

                        col.Item().PaddingTop(20);

                        // ── NET PAY BOX ──
                        col.Item().Background(Color.FromHex("#16A34A"))
                            .Padding(14).Row(row =>
                            {
                                row.RelativeItem()
                                    .Text("NET PAY")
                                    .Bold().FontSize(14)
                                    .FontColor(Colors.White);
                                row.RelativeItem().AlignRight()
                                    .Text($"₦{payslip.NetPay:N2}")
                                    .Bold().FontSize(14)
                                    .FontColor(Colors.White);
                            });

                        // Employer pension note
                        col.Item().PaddingTop(10)
                            .Text($"* Employer Pension Contribution (10%): " +
                                  $"₦{payslip.EmployerPension:N2} " +
                                  $"— paid directly by employer")
                            .FontSize(8)
                            .FontColor(Color.FromHex("#374151"));
                    });

                    // ── FOOTER ──
                    page.Footer().Column(col =>
                    {
                        col.Item().LineHorizontal(1)
                            .LineColor(Color.FromHex("#2563EB"));
                        col.Item().PaddingTop(6).AlignCenter()
                            .Text($"This payslip is computer generated " +
                                  $"and requires no signature. " +
                                  $"Generated on " +
                                  $"{DateTime.UtcNow:dd MMM yyyy HH:mm} UTC")
                            .FontSize(8)
                            .FontColor(Color.FromHex("#374151"));
                    });
                });
            });

            var pdfBytes = document.GeneratePdf();
            return Task.FromResult(pdfBytes);
        }
    }
}