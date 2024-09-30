using System.Text;

/// <summary>
/// �ȒP�ȃV�[�U�[�Í����N���X
/// </summary>
public static class CaesarCipher
{
    static string Process(string src, int delta)
    {
        var stringBuilder = new StringBuilder();
        for (int i = 0; i < src.Length; i++)
        {
            stringBuilder.Append((char)(src[i] + delta));
        }
        return stringBuilder.ToString();
    }

    public static string Encrypt(string src, int delta)
    {
        return Process(src, delta);
    }

    public static string Decrypt(string src, int delta)
    {
        return Process(src, -delta);
    }
}