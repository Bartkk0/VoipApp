namespace Common; 

public class Base32Convert {
    public static string Encode(byte[] bytes) {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        string output = "";
        for (int bitIndex = 0; bitIndex < bytes.Length * 8; bitIndex += 5) {
            int dualbyte = bytes[bitIndex / 8] << 8;
            if (bitIndex / 8 + 1 < bytes.Length)
                dualbyte |= bytes[bitIndex / 8 + 1];
            dualbyte = 0x1f & (dualbyte >> (16 - bitIndex % 8 - 5));
            output += alphabet[dualbyte];
        }

        return output;
    }

    public static byte[] Decode(string base32) {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        List<byte> output = new List<byte>();
        char[] bytes = base32.ToCharArray();
        for (int bitIndex = 0; bitIndex < base32.Length * 5; bitIndex += 8) {
            int dualbyte = alphabet.IndexOf(bytes[bitIndex / 5]) << 10;
            if (bitIndex / 5 + 1 < bytes.Length)
                dualbyte |= alphabet.IndexOf(bytes[bitIndex / 5 + 1]) << 5;
            if (bitIndex / 5 + 2 < bytes.Length)
                dualbyte |= alphabet.IndexOf(bytes[bitIndex / 5 + 2]);

            dualbyte = 0xff & (dualbyte >> (15 - bitIndex % 5 - 8));
            output.Add((byte)(dualbyte));
        }
        return output.ToArray();
    }
}