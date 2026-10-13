using System.Text;

namespace Sendify.MessagesServiceSmsDigiWr21;

public static class PduEncoder
{
    public static string EncodePdu(string phoneNumber, string message, out int pduLength)
    {
        var sb = new StringBuilder();
        
        // SMSC (Service Center Address) - 00 means use default from SIM
        sb.Append("00");
        
        // PDU Type (SMS-SUBMIT, no status report request)
        sb.Append("11");
        
        // Message Reference (00 = let phone set)
        sb.Append("00");
        
        // Destination Address (phone number)
        var encodedPhone = EncodePhoneNumber(phoneNumber);
        sb.Append(encodedPhone);
        
        // Protocol Identifier (00 = standard)
        sb.Append("00");
        
        // Data Coding Scheme
        bool useUnicode = RequiresUnicode(message);
        if (useUnicode)
        {
            // 08 = UCS-2 16-bit encoding
            sb.Append("08");
        }
        else
        {
            // 00 = 7-bit default alphabet
            sb.Append("00");
        }
        
        // Validity Period (AA = 24 hours)
        sb.Append("AA");
        
        // User Data Length and User Data
        string encodedMessage;
        int udl;
        
        if (useUnicode)
        {
            encodedMessage = EncodeUcs2(message);
            udl = message.Length * 2; // UCS-2 uses 2 bytes per character
        }
        else
        {
            encodedMessage = Encode7Bit(message);
            udl = message.Length; // 7-bit reports character count
        }
        
        sb.Append(udl.ToString("X2"));
        sb.Append(encodedMessage);
        
        // PDU length is the number of octets (bytes) excluding SMSC length
        // Calculate: (total hex digits - SMSC part) / 2
        pduLength = (sb.Length - 2) / 2;
        
        return sb.ToString();
    }
    
    private static string EncodePhoneNumber(string phone)
    {
        // Remove non-digit characters
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        
        // Add international prefix if not present
        if (!digits.StartsWith("00") && !digits.StartsWith("+"))
        {
            digits = "48" + digits; // Default to Poland country code
        }
        else if (digits.StartsWith("00"))
        {
            digits = digits.Substring(2);
        }
        else if (digits.StartsWith("+"))
        {
            digits = digits.Substring(1);
        }
        
        // Type of address (91 = international format)
        var typeOfAddress = "91";
        
        // Length of phone number
        var length = digits.Length;
        
        // Pad with F if odd length
        if (length % 2 != 0)
        {
            digits += "F";
        }
        
        // Swap nibbles (semi-octet representation)
        var swapped = new StringBuilder();
        for (int i = 0; i < digits.Length; i += 2)
        {
            swapped.Append(digits[i + 1]);
            swapped.Append(digits[i]);
        }
        
        return length.ToString("X2") + typeOfAddress + swapped.ToString();
    }
    
    private static string Encode7Bit(string message)
    {
        var bytes = new List<byte>();
        int shift = 0;
        int carry = 0;
        
        foreach (char c in message)
        {
            int code = CharTo7Bit(c);
            
            if (shift == 7)
            {
                shift = 0;
                carry = 0;
                continue;
            }
            
            int current = ((code << shift) | carry) & 0xFF;
            bytes.Add((byte)current);
            
            carry = code >> (7 - shift);
            shift++;
        }
        
        if (shift > 0 && carry > 0)
        {
            bytes.Add((byte)carry);
        }
        
        return BitConverter.ToString(bytes.ToArray()).Replace("-", "");
    }
    
    private static string EncodeUcs2(string message)
    {
        var bytes = Encoding.BigEndianUnicode.GetBytes(message);
        return BitConverter.ToString(bytes).Replace("-", "");
    }
    
    private static bool RequiresUnicode(string message)
    {
        // Check if message contains characters outside GSM 7-bit alphabet
        foreach (char c in message)
        {
            if (!IsGsm7BitCharacter(c))
            {
                return true;
            }
        }
        return false;
    }
    
    private static bool IsGsm7BitCharacter(char c)
    {
        // GSM 7-bit default alphabet (basic characters)
        if (c >= 'A' && c <= 'Z') return true;
        if (c >= 'a' && c <= 'z') return true;
        if (c >= '0' && c <= '9') return true;
        
        // Common symbols in GSM alphabet
        string gsmChars = "@£$¥èéùìòÇ\nØø\rÅåΔ_ΦΓΛΩΠΨΣΘΞÆæßÉ !\"#¤%&'()*+,-./:<=>?¡ÄÖÑÜ§¿äöñüà";
        
        return gsmChars.Contains(c) || c == ' ' || c == '\r' || c == '\n';
    }
    
    private static int CharTo7Bit(char c)
    {
        // Basic GSM 7-bit alphabet mapping
        if (c >= 'A' && c <= 'Z') return c - 'A' + 0x41;
        if (c >= 'a' && c <= 'z') return c - 'a' + 0x61;
        if (c >= '0' && c <= '9') return c - '0' + 0x30;
        
        // Special characters
        switch (c)
        {
            case '@': return 0x00;
            case '£': return 0x01;
            case '$': return 0x02;
            case '¥': return 0x03;
            case 'è': return 0x04;
            case 'é': return 0x05;
            case 'ù': return 0x06;
            case 'ì': return 0x07;
            case 'ò': return 0x08;
            case 'Ç': return 0x09;
            case '\n': return 0x0A;
            case 'Ø': return 0x0B;
            case 'ø': return 0x0C;
            case '\r': return 0x0D;
            case 'Å': return 0x0E;
            case 'å': return 0x0F;
            case 'Δ': return 0x10;
            case '_': return 0x11;
            case 'Φ': return 0x12;
            case 'Γ': return 0x13;
            case 'Λ': return 0x14;
            case 'Ω': return 0x15;
            case 'Π': return 0x16;
            case 'Ψ': return 0x17;
            case 'Σ': return 0x18;
            case 'Θ': return 0x19;
            case 'Ξ': return 0x1A;
            case 'Æ': return 0x1C;
            case 'æ': return 0x1D;
            case 'ß': return 0x1E;
            case 'É': return 0x1F;
            case ' ': return 0x20;
            case '!': return 0x21;
            case '"': return 0x22;
            case '#': return 0x23;
            case '¤': return 0x24;
            case '%': return 0x25;
            case '&': return 0x26;
            case '\'': return 0x27;
            case '(': return 0x28;
            case ')': return 0x29;
            case '*': return 0x2A;
            case '+': return 0x2B;
            case ',': return 0x2C;
            case '-': return 0x2D;
            case '.': return 0x2E;
            case '/': return 0x2F;
            case ':': return 0x3A;
            case ';': return 0x3B;
            case '<': return 0x3C;
            case '=': return 0x3D;
            case '>': return 0x3E;
            case '?': return 0x3F;
            case '¡': return 0x40;
            case 'Ä': return 0x5B;
            case 'Ö': return 0x5C;
            case 'Ñ': return 0x5D;
            case 'Ü': return 0x5E;
            case '§': return 0x5F;
            case '¿': return 0x60;
            case 'ä': return 0x7B;
            case 'ö': return 0x7C;
            case 'ñ': return 0x7D;
            case 'ü': return 0x7E;
            case 'à': return 0x7F;
            default: return 0x20; // Space as fallback
        }
    }
    
    public static int GetMaxMessageLength(string message)
    {
        // UCS-2: 70 characters per SMS (140 bytes / 2)
        // 7-bit: 160 characters per SMS
        bool useUnicode = RequiresUnicode(message);
        return useUnicode ? 70 : 160;
    }
}
