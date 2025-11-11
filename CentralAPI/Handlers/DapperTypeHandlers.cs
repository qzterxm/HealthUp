using Dapper;
using System.Data;

namespace DataAccess.DataAccess
{
    public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
    {
        public override DateOnly Parse(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return default;
            }

            if (value is DateTime dateTime)
            {
                return DateOnly.FromDateTime(dateTime);
            }

            if (value is string str)
            {
                return DateOnly.Parse(str);
            }

            throw new ArgumentException($"Cannot convert {value} to DateOnly");
        }

        public override void SetValue(IDbDataParameter parameter, DateOnly value)
        {
            parameter.Value = value.ToString("yyyy-MM-dd");
        }
    }

    public class NullableDateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly?>
    {
        public override DateOnly? Parse(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return null;
            }

            if (value is DateTime dateTime)
            {
                return DateOnly.FromDateTime(dateTime);
            }

            if (value is string str)
            {
                return DateOnly.Parse(str);
            }

            return null;
        }

        public override void SetValue(IDbDataParameter parameter, DateOnly? value)
        {
            if (value.HasValue)
            {
                parameter.Value = value.Value.ToString("yyyy-MM-dd");
            }
            else
            {
                parameter.Value = DBNull.Value;
            }
        }
    }

    public class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
    {
        public override Guid Parse(object value)
        {
            if (value is Guid guid) return guid;
            if (value is string str) return Guid.Parse(str);
            if (value is byte[] bytes) return new Guid(bytes);
            throw new ArgumentException($"Cannot convert {value} to Guid");
        }

        public override void SetValue(IDbDataParameter parameter, Guid value)
        {
            parameter.Value = value.ToString();
        }
    }

    public class NullableGuidTypeHandler : SqlMapper.TypeHandler<Guid?>
    {
        public override Guid? Parse(object value)
        {
            if (value == null || value == DBNull.Value) return null;
            if (value is Guid guid) return guid;
            if (value is string str && !string.IsNullOrEmpty(str)) return Guid.Parse(str);
            if (value is byte[] bytes) return new Guid(bytes);
            return null;
        }

        public override void SetValue(IDbDataParameter parameter, Guid? value)
        {
            parameter.Value = value?.ToString() ?? (object)DBNull.Value;
        }
    }
}