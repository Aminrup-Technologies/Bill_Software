using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bill_Software
{
    public class MoneyConvDS
    {
        public static string MoneyConvFn(string MVal)
        {
            string TxtMon = MVal;
            string iout = "";

            if (TxtMon.Trim().Length > 18)
            {
                iout = "Maximum 18 digits only Allowed";
            }
            else
            {
                iout = NToW(TxtMon.Trim());
            }

            return iout;
        }

        private static string One(Int64 x)
        {
            switch (x)
            {
                case 1:
                    return "One";
                case 2:
                    return "Two";
                case 3:
                    return "Three";
                case 4:
                    return "Four";
                case 5:
                    return "Five";
                case 6:
                    return "Six";
                case 7:
                    return "Seven";
                case 8:
                    return "Eight";
                case 9:
                    return "Nine";
                default:
                    return "";

            }
        }

        private static string two(Int64 x, Int64 y)
        {
            switch (x)
            {
                case 1:
                    switch (y)
                    {
                        case 0:
                            return "Ten";
                        case 1:
                            return "Eleven";
                        case 2:
                            return "Twelve";
                        case 3:
                            return "Thirteen";
                        case 4:
                            return "Fourteen";
                        case 5:
                            return "Fifteen";
                        case 6:
                            return "Sixteen";
                        case 7:
                            return "Seventeen";
                        case 8:
                            return "Eighteen";
                        case 9:
                            return "Nineteen";
                        default:
                            return "";
                    }
                case 2:
                    return "Twenty ";
                case 3:
                    return "Thirty ";
                case 4:
                    return "Forty ";
                case 5:
                    return "Fifty ";
                case 6:
                    return "Sixty ";
                case 7:
                    return "Seventy ";
                case 8:
                    return "Eighty ";
                case 9:
                    return "Ninety ";
                default:
                    return "";
            }
        }

        private static string three(Int64 x)
        {
            if (x != 0)  //to avoid empty hundred for 1000
            {
                string xx = One(x) + " Hundred ";
                return xx;
            }
            else
                return "";
        }

        private static string frfv(Int64 x, Int64 y)
        {
            if (x == 0 && y == 0)
            {
                return "";
            }
            else
            {
                if (x != 0)
                {
                    if (x != 1)
                    {
                        return two(x, y) + One(y) + " Thousand ";
                    }
                    else
                    {
                        return two(x, y) + " Thousand ";
                    }
                }
                else
                {
                    return One(y) + " Thousand ";
                }
            }
        }

        private static string sxsn(Int64 x, Int64 y)
        {
            if (x == 0 && y == 0)
            {
                return "";
            }
            else
            {
                if (x != 0)
                {
                    if (x != 1)
                    {
                        return two(x, y) + One(y) + " Lacs ";
                    }
                    else
                    {
                        return two(x, y) + " Lacs ";
                    }
                }
                else
                {
                    return One(y) + " Lacs ";
                }
            }
        }

        private static string etnn(Int64 x, Int64 y)
        {
            if (x == 0 && y == 0)
            {
                return "";
            }
            else
            {
                if (x != 0)
                {
                    if (x != 1)
                    {
                        return two(x, y) + One(y) + " Crores ";
                    }
                    else
                    {
                        return two(x, y) + " Crores ";
                    }
                }
                else
                {
                    return One(y) + " Crores ";
                }
            }
        }

        private static string NToWConv(String aaa)
        {
            Int64[] no = new Int64[10];
            int k;
            string a = "";
            k = aaa.Length;
            for (int j = 0; j < 7; j++)
            {
                if (k > 0)
                {
                    no[j] = Convert.ToInt64(aaa.Substring(k - 1, 1));
                }
                else
                {
                    no[j] = 0;
                }
                k = k - 1;
            }

            a = a + sxsn(no[6], no[5]);
            a = a + frfv(no[4], no[3]);
            a = a + three(no[2]);
            //if (no[1] != 0 || no[0] != 0)
            //{
            //    a = a + " and ";
            //}
            if (a.Trim().Length > 0)
            {
                if (no[1] != 0 || no[0] != 0)
                {
                    a = a + " & ";
                }
            }

            a = a + two(no[1], no[0]);
            if (no[1] != 1)
            {
                a = a + One(no[0]);
            }
            return a;
        }

        public static string NToW_old(String ParaNum)
        {
            //*****************************************
            //Int64 can have maximum value as 9,223,372,036,854,775,807.
            if (ParaNum.Length > 18)
            {
                return "Maximum 18 digits only Allowed";
            }
            Decimal a1 = Convert.ToDecimal(ParaNum);
            Int64 a2 = (Int64)Decimal.Floor(a1);
            Decimal a3 = a1 - a2;
            if (a2.ToString().Length > 18)
            {
                return "Maximum 18 digits only Allowed";
            }

            Int64 Part1, Part2, Part3, TempV1, TempV2;
            TempV1 = TempV2 = Part1 = Part2 = Part3 = 0;
            Int64 Fr = (Int64)(a3 * 100);
            TempV1 = a2;

            TempV2 = (Int64)Decimal.Floor(TempV1 / 10000000);
            Part1 = TempV1 - TempV2 * 10000000;
            TempV1 = TempV2;

            if (TempV1 >= 10000000)
            {
                TempV2 = (Int64)Decimal.Floor(TempV1 / 10000000);
                Part2 = TempV1 - TempV2 * 10000000;
                TempV1 = TempV2;
            }
            else
            {
                Part2 = TempV1;
                TempV1 = 0;
            }

            if (TempV1 >= 10000000)
            {
                TempV2 = (Int64)Decimal.Floor(TempV1 / 10000000);
                Part3 = TempV1 - TempV2 * 10000000;
                TempV1 = TempV2;
            }
            else
            {
                Part3 = TempV1;
                TempV1 = 0;
            }
            if (TempV1 >= 10000000)
            {
                return "Rupees Conversion Error: Number exceeds the Length 21 digits";
            }

            string NName = "";
            int ln = NName.Trim().Length;

            if (Part3 > 0)
            {
                NName = NName.Trim() + NToWConv(Part3.ToString()) + " Crore Crore";
            }
            if (Part2 > 0)
            {
                if (Part2 == 1)
                {
                    NName = NName.Trim() + "" + NToWConv(Part2.ToString()) + " Crore";
                }
                else
                {
                    NName = NName.Trim() + "" + NToWConv(Part2.ToString()) + " Crores";
                }
            }
            if (Part1 > 0)
            {
                NName = NName.Trim() + "" + NToWConv(Part1.ToString());
            }
            if (NName.Trim().Length > 0)
            {
                NName = "Rs " + NName;
            }
            if (Fr > 0)
            {
                if (NName.Trim().Length > 0)
                {
                    NName = NName.Trim() + " & " + NToWConv(Fr.ToString());
                }
                else
                {
                    NName = NToWConv(Fr.ToString()) + " Paise";
                }
            }
            if (NName.Trim().Length > 0)
            {
                NName = NName + " Only.";
            }
            return NName;
        }

        public static string NToW(string ParaNum)
        {
            // Validate input length
            if (ParaNum.Length > 18)
                return "Maximum 18 digits only allowed";

            decimal a1 = 0;
            // Ensure input is numeric
            if (!decimal.TryParse(ParaNum, out a1))
                return "Invalid number format";

            // Split into integer and fractional parts
            long integerPart = (long)Math.Floor(a1);
            int fractionalPart = (int)((a1 - integerPart) * 100); // Consider two decimal places as paise

            // Validate integer part length
            if (integerPart.ToString().Length > 18)
                return "Maximum 18 digits only allowed";

            // Split the integer part into components: Crores, Lakhs, and Remaining
            long crores = integerPart / 10000000;
            long remainingAfterCrores = integerPart % 10000000;
            long lakhs = remainingAfterCrores / 100000;
            long remaining = remainingAfterCrores % 100000;

            // Build the words for the integer part
            string result = "";

            if (crores > 0)
                result += NToWConv(crores.ToString()) + (crores == 1 ? " Crore" : " Crores");

            if (lakhs > 0)
            {
                if (result.Length > 0) result += " ";
                result += NToWConv(lakhs.ToString()) + (lakhs == 1 ? " Lakh" : " Lakhs");
            }

            if (remaining > 0)
            {
                if (result.Length > 0) result += " ";
                result += NToWConv(remaining.ToString());
            }

            if (!string.IsNullOrWhiteSpace(result))
                result = "Rs " + result;

            // Add fractional part as paise
            if (fractionalPart > 0)
            {
                if (!string.IsNullOrWhiteSpace(result))
                    result += " & ";

                result += NToWConv(fractionalPart.ToString()) + (fractionalPart == 1 ? " Paise" : " Paise");
            }

            // Finalize result
            if (!string.IsNullOrWhiteSpace(result))
                result += " Only.";

            return result;
        }

    }
}