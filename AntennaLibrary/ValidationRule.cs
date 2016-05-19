using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AntennaLibrary
{
    public class NumOfBandsValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!string.IsNullOrEmpty(value as string))
            {
                int numOfBands;
                if (!int.TryParse((string)value, out numOfBands) || numOfBands < 1 || numOfBands > 10)
                {
                    return new ValidationResult(false, "Number of bands must be between 1 and 10");
                }
            }
            return new ValidationResult(true, null);
        }
    }

    public class GainValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!string.IsNullOrEmpty(value as string))
            {
                double gain;
                if (!double.TryParse((string) value, out gain) || gain < 0)
                {
                    return new ValidationResult(false, "Gain must be no less than 0");
                }
            }
            return new ValidationResult(true, null);
        }
    }

    public class _3dBWidthValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!string.IsNullOrEmpty(value as string))
            {
                double _3dBWidth;
                if (!double.TryParse((string)value, out _3dBWidth) || _3dBWidth < 0 || _3dBWidth > 360)
                {
                    return new ValidationResult(false, "3dB Width must be from 0 to 360");
                }
            }
            return new ValidationResult(true, null);
        }
    }

    public class VSWRValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!string.IsNullOrEmpty(value as string))
            {
                double vswr;
                if (!double.TryParse((string)value, out vswr) || vswr < 1)
                {
                    return new ValidationResult(false, "VSWR must be no less than 1");
                }
            }
            return new ValidationResult(true, null);
        }
    }

    public class CrossPolarizationValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!string.IsNullOrEmpty(value as string))
            {
                double crossPolarization;
                if (!double.TryParse((string)value, out crossPolarization) || crossPolarization >= 0)
                {
                    return new ValidationResult(false, "Cross Polarization must be less than 0");
                }
            }
            return new ValidationResult(true, null);
        }
    }
}
