namespace DemoSat2016Netduino_OnboardSD.Work_Items {
    public enum PacketType : byte {
        StartByte = 0xFF,
        BmpDump = 0x00,
        MagDump = 0x01,
        BnoDump = 0x02,
        TimeSync = 0x03,
        DebugMessage = 0x04,
    }
}