using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace acgalleryapi.ViewModels
{
    public class PhotoViewModelBase : BaseViewModel
    {
        [Required]
        [StringLength(40)]
        public String PhotoId { get; set; }
        [StringLength(50)]
        public String Title { get; set; }
        [StringLength(100)]
        public String Desp { get; set; }
        public Int32 Width { get; set; }
        public Int32 Height { get; set; }
        public Int32 ThumbWidth { get; set; }
        public Int32 ThumbHeight { get; set; }
        [StringLength(100)]
        public String FileUrl { get; set; }
        [StringLength(100)]
        public String ThumbnailFileUrl { get; set; }
        public String FileFormat { get; set; }
        public DateTime UploadedTime { get; set; }
        [StringLength(50)]
        public String UploadedBy { get; set; }
        [StringLength(100)]
        public String OrgFileName { get; set; }
        public Boolean IsOrgThumbnail { get; set; }
        public Boolean IsPublic { get; set; }
        [StringLength(50)]
        public String CameraMaker { get; set; }
        [StringLength(100)]
        public String CameraModel { get; set; }
        [StringLength(100)]
        public string LensModel { get; set; }
        [StringLength(20)]
        public String AVNumber { get; set; }
        [StringLength(50)]
        public string ShutterSpeed { get; set; }
        public Int32 ISONumber { get; set; }
    }

    public class PhotoViewModel : PhotoViewModelBase
    {
        public List<ExifTagItem> ExifTags = new List<ExifTagItem>();
        public List<String> Tags = new List<string>();
        public Double AvgRating { get; set; }
    }

    public class PhotoViewModelEx : PhotoViewModel
    {
        // This class adds the information for FineUploader required:
        // success: success flag
        // error: error message

        public PhotoViewModelEx(Boolean bSuc, String strErr = "")
        {
            success = bSuc;
            error = strErr;
        }

        public Boolean success;
        public String error;
    }

    public class PhotoTagViewModel
    {
        [Required]
        [StringLength(40)]
        public String PhotoId { get; set; }
        [Required]
        [StringLength(50)]
        public String TagString { get; set; }

    }

    public enum GeneralFilterOperatorEnum
    {
        Equal = 1,
        NotEqual = 2,
        Between = 3,
        LargerThan = 4,
        LargerEqual = 5,
        LessThan = 6,
        LessEqual = 7,
        Like = 8,
    }

    public enum GeneralFilterValueEnum
    {
        Number = 1,
        String = 2,
        Date = 3,
        Boolean = 4
    }

    public class GeneralFilterItem
    {
        public string FieldName { get; set; }
        public GeneralFilterOperatorEnum Operator { get; set; }
        public string LowValue { get; set; }
        public string HighValue { get; set; }
        public GeneralFilterValueEnum ValueType { get; set; }

        public GeneralFilterItem()
        {
            this.Operator = GeneralFilterOperatorEnum.Equal;
            this.ValueType = GeneralFilterValueEnum.String; // Default is string
        }

        public Boolean IsValid()
        {
            if (String.IsNullOrEmpty(FieldName))
                return false;

            switch (this.Operator)
            {
                case GeneralFilterOperatorEnum.Equal:
                case GeneralFilterOperatorEnum.NotEqual:
                    {
                        if (String.IsNullOrEmpty(LowValue))
                            return false;
                    }
                    break;

                case GeneralFilterOperatorEnum.LargerEqual:
                case GeneralFilterOperatorEnum.LargerThan:
                case GeneralFilterOperatorEnum.LessEqual:
                case GeneralFilterOperatorEnum.LessThan:
                    {
                        if (String.IsNullOrEmpty(LowValue))
                            return false;

                        if (ValueType == GeneralFilterValueEnum.Boolean
                            || ValueType == GeneralFilterValueEnum.String)
                            return false;
                    }
                    break;

                case GeneralFilterOperatorEnum.Between:
                    {
                        if (String.IsNullOrEmpty(LowValue))
                            return false;
                        if (String.IsNullOrEmpty(HighValue))
                            return false;

                        if (ValueType == GeneralFilterValueEnum.Boolean
                            || ValueType == GeneralFilterValueEnum.String)
                            return false;
                    }
                    break;

                case GeneralFilterOperatorEnum.Like:
                    {
                        if (String.IsNullOrEmpty(LowValue))
                            return false;

                        if (ValueType != GeneralFilterValueEnum.String)
                            return false;
                    }
                    break;

                default:
                    return false;
            }

            return true;
        }

        public string GenerateSql()
        {
            if (!this.IsValid())
                return String.Empty;

            String strRst = " ";
            switch (this.Operator)
            {
                case GeneralFilterOperatorEnum.Equal:
                    {
                        switch (this.ValueType)
                        {
                            case GeneralFilterValueEnum.Number:
                                strRst = FieldName + " = " + LowValue;
                                break;

                            case GeneralFilterValueEnum.Date:
                                strRst = FieldName + " = '" + LowValue + "' ";
                                break;

                            case GeneralFilterValueEnum.Boolean:
                                strRst = FieldName + " = " + (Boolean.Parse(LowValue) ? "1" : "0 ");
                                break;

                            case GeneralFilterValueEnum.String:
                            default:
                                strRst = FieldName + " = N'" + LowValue + "' ";
                                break;
                        }
                    }
                    break;

                case GeneralFilterOperatorEnum.NotEqual:
                    {
                        switch (this.ValueType)
                        {
                            case GeneralFilterValueEnum.Number:
                                strRst = FieldName + " <> " + LowValue;
                                break;

                            case GeneralFilterValueEnum.Date:
                                strRst = FieldName + " <> '" + LowValue + "' ";
                                break;

                            case GeneralFilterValueEnum.Boolean:
                                strRst = FieldName + " <> " + (Boolean.Parse(LowValue) ? "1" : "0 ");
                                break;

                            case GeneralFilterValueEnum.String:
                            default:
                                strRst = FieldName + " <> N'" + LowValue + "' ";
                                break;
                        }
                    }
                    break;

                case GeneralFilterOperatorEnum.LargerEqual:
                    {
                        switch (this.ValueType)
                        {
                            case GeneralFilterValueEnum.Number:
                                strRst = FieldName + " >= " + LowValue;
                                break;

                            case GeneralFilterValueEnum.Date:
                                strRst = FieldName + " >= '" + LowValue + "' ";
                                break;

                            case GeneralFilterValueEnum.Boolean:
                                throw new Exception("Unsupported operator on Boolean");

                            case GeneralFilterValueEnum.String:
                            default:
                                strRst = FieldName + " >= N'" + LowValue + "' ";
                                break;
                        }
                    }
                    break;

                case GeneralFilterOperatorEnum.LargerThan:
                    {
                        switch (this.ValueType)
                        {
                            case GeneralFilterValueEnum.Number:
                                strRst = FieldName + " > " + LowValue;
                                break;

                            case GeneralFilterValueEnum.Date:
                                strRst = FieldName + " > '" + LowValue + "' ";
                                break;

                            case GeneralFilterValueEnum.Boolean:
                                throw new Exception("Unsupported operator on Boolean");

                            case GeneralFilterValueEnum.String:
                            default:
                                strRst = FieldName + " > N'" + LowValue + "' ";
                                break;
                        }
                    }
                    break;

                case GeneralFilterOperatorEnum.LessEqual:
                    {
                        switch (this.ValueType)
                        {
                            case GeneralFilterValueEnum.Number:
                                strRst = FieldName + " <= " + LowValue;
                                break;

                            case GeneralFilterValueEnum.Date:
                                strRst = FieldName + " <= '" + LowValue + "' ";
                                break;

                            case GeneralFilterValueEnum.Boolean:
                                throw new Exception("Unsupported operator on Boolean");

                            case GeneralFilterValueEnum.String:
                            default:
                                strRst = FieldName + " <= N'" + LowValue + "' ";
                                break;
                        }
                    }
                    break;

                case GeneralFilterOperatorEnum.LessThan:
                    {
                        switch (this.ValueType)
                        {
                            case GeneralFilterValueEnum.Number:
                                strRst = FieldName + " < " + LowValue;
                                break;

                            case GeneralFilterValueEnum.Date:
                                strRst = FieldName + " < '" + LowValue + "' ";
                                break;

                            case GeneralFilterValueEnum.Boolean:
                                throw new Exception("Unsupported operator on Boolean");

                            case GeneralFilterValueEnum.String:
                            default:
                                strRst = FieldName + " < N'" + LowValue + "' ";
                                break;
                        }
                    }
                    break;

                case GeneralFilterOperatorEnum.Between:
                    {
                        switch (this.ValueType)
                        {
                            case GeneralFilterValueEnum.Number:
                                strRst = FieldName + " >= " + LowValue + " AND " + FieldName + " <= " + HighValue;
                                break;

                            case GeneralFilterValueEnum.Date:
                                strRst = FieldName + " >= '" + LowValue + "' AND " + FieldName + " <= '" + HighValue + "' ";
                                break;

                            case GeneralFilterValueEnum.Boolean:
                                throw new Exception("Unsupported operator on Boolean");

                            case GeneralFilterValueEnum.String:
                            default:
                                strRst = FieldName + " >= N'" + LowValue + "' AND " + FieldName + " <= N'" + HighValue + "' ";
                                break;
                        }
                    }
                    break;

                case GeneralFilterOperatorEnum.Like:
                    {
                        if (ValueType == GeneralFilterValueEnum.String)
                        {
                            if (LowValue.Contains("%"))
                                strRst = FieldName + " LIKE N'" + LowValue + "' ";
                            else
                                strRst = FieldName + " LIKE N'%" + LowValue + "%' ";
                        }                            
                    }
                    break;

                default:
                    break;
            }

            return strRst;
        }
    }

    public class PhotoSearchFilterViewModel
    {
        public List<GeneralFilterItem> FieldList { get; private set; }

        public PhotoSearchFilterViewModel()
        {
            this.FieldList = new List<GeneralFilterItem>();
        }
    }
}
