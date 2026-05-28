namespace PayrollAPI.Helpers
{
    public class TaxResult
    {
        public decimal GrossSalary { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal TotalAllowances { get; set; }

        // Deductions
        public decimal EmployeePension { get; set; }   // 8% of gross
        public decimal EmployerPension { get; set; }   // 10% of gross
        public decimal NHF { get; set; }               // 2.5% of basic
        public decimal PAYE { get; set; }              // Income tax

        public decimal TotalDeductions { get; set; }
        public decimal NetPay { get; set; }
    }

    public static class TaxCalculator
    {
        public static TaxResult Calculate(
            decimal basicSalary,
            decimal housingAllowance,
            decimal transportAllowance,
            decimal mealAllowance,
            decimal otherAllowances)
        {
            // ── STEP 1: Calculate Gross Salary ──
            decimal totalAllowances = housingAllowance + transportAllowance
                                    + mealAllowance + otherAllowances;
            decimal grossSalary = basicSalary + totalAllowances;

            // ── STEP 2: Pension (PRA 2014) ──
            // Employee contributes 8% of gross
            decimal employeePension = Math.Round(grossSalary * 0.08m, 2);
            // Employer contributes 10% of gross
            decimal employerPension = Math.Round(grossSalary * 0.10m, 2);

            // ── STEP 3: NHF (National Housing Fund) ──
            // 2.5% of basic salary only
            decimal nhf = Math.Round(basicSalary * 0.025m, 2);

            // ── STEP 4: Calculate Taxable Income for PAYE ──
            // Taxable income = Gross - Pension - NHF - CRA
            // CRA = Consolidated Relief Allowance
            // CRA = higher of (N200,000 or 1% of gross) + 20% of gross
            decimal craBase = Math.Max(200000m, grossSalary * 0.01m);
            decimal cra = craBase + (grossSalary * 0.20m);

            decimal taxableIncome = grossSalary - employeePension - nhf - cra;
            taxableIncome = Math.Max(0, taxableIncome); // cannot be negative

            // ── STEP 5: PAYE Tax (Nigerian Tax Bands - Annual) ──
            // Convert monthly taxable income to annual for band calculation
            decimal annualTaxableIncome = taxableIncome * 12;
            decimal annualPAYE = CalculateAnnualPAYE(annualTaxableIncome);

            // Convert back to monthly
            decimal monthlyPAYE = Math.Round(annualPAYE / 12, 2);

            // ── STEP 6: Total Deductions and Net Pay ──
            decimal totalDeductions = employeePension + nhf + monthlyPAYE;
            decimal netPay = grossSalary - totalDeductions;

            return new TaxResult
            {
                GrossSalary = grossSalary,
                BasicSalary = basicSalary,
                TotalAllowances = totalAllowances,
                EmployeePension = employeePension,
                EmployerPension = employerPension,
                NHF = nhf,
                PAYE = monthlyPAYE,
                TotalDeductions = totalDeductions,
                NetPay = netPay
            };
        }

        private static decimal CalculateAnnualPAYE(decimal annualTaxableIncome)
        {
            // Nigerian PAYE Tax Bands (FIRS 2024)
            // First  ₦300,000   →  7%
            // Next   ₦300,000   →  11%
            // Next   ₦500,000   →  15%
            // Next   ₦500,000   →  19%
            // Next   ₦1,600,000 →  21%
            // Above  ₦3,200,000 →  24%

            decimal tax = 0;
            decimal remaining = annualTaxableIncome;

            var bands = new[]
            {
                (limit: 300_000m,   rate: 0.07m),
                (limit: 300_000m,   rate: 0.11m),
                (limit: 500_000m,   rate: 0.15m),
                (limit: 500_000m,   rate: 0.19m),
                (limit: 1_600_000m, rate: 0.21m),
                (limit: decimal.MaxValue, rate: 0.24m)
            };

            foreach (var band in bands)
            {
                if (remaining <= 0) break;

                decimal taxable = Math.Min(remaining, band.limit);
                tax += taxable * band.rate;
                remaining -= taxable;
            }

            return Math.Round(tax, 2);
        }
    }
}