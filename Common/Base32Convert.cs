namespace Common;

public class Base32Convert {
    public static string Encode(byte[] bytes) {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var output = "";
        for (var bitIndex = 0; bitIndex < bytes.Length * 8; bitIndex += 5) {
            var dualbyte = bytes[bitIndex / 8] << 8;
            if (bitIndex / 8 + 1 < bytes.Length)
                dualbyte |= bytes[bitIndex / 8 + 1];
            dualbyte = 0x1f & (dualbyte >> (16 - bitIndex % 8 - 5));
            output += alphabet[dualbyte];
        }

        return output;
    }

    public static byte[] Decode(string base32) {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var output = new List<byte>();
        var bytes = base32.ToCharArray();
        for (var bitIndex = 0; bitIndex < base32.Length * 5; bitIndex += 8) {
            var dualbyte = alphabet.IndexOf(bytes[bitIndex / 5]) << 10;
            if (bitIndex / 5 + 1 < bytes.Length)
                dualbyte |= alphabet.IndexOf(bytes[bitIndex / 5 + 1]) << 5;
            if (bitIndex / 5 + 2 < bytes.Length)
                dualbyte |= alphabet.IndexOf(bytes[bitIndex / 5 + 2]);

            dualbyte = 0xff & (dualbyte >> (15 - bitIndex % 5 - 8));
            output.Add((byte)dualbyte);
        }

        return output.ToArray();
    }
}