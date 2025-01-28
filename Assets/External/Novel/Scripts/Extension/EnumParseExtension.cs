using System;

namespace Novel
{
    public static class EnumParseExtension
    {
        /// <summary>
        /// �������enum�ɕϊ����܂�
        /// </summary>
        /// <typeparam name="TEnum">enum�̌^</typeparam>
        /// <param name="s">�ϊ����镶����</param>
        /// <param name="enm">out�̌���</param>
        /// <returns>�ϊ��ɐ���������</returns>
        public static bool TryParseToEnum<TEnum>(this string s, out TEnum enm)
            where TEnum : struct, Enum, IComparable, IFormattable, IConvertible   // �R���p�C�����ɂł��邾������
        {
            if (typeof(TEnum).IsEnum)
            {
                return Enum.TryParse(s, out enm) && Enum.IsDefined(typeof(TEnum), enm);
            }
            else //���s���`�F�b�N��enum����Ȃ������ꍇ
            {
                enm = default;
                return false;
            }
        }
    }
}