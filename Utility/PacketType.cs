namespace DemoSat2016Netduino_OnboardSD.Work_Items {
    public enum PacketType : byte {
        StartByte = 0xFF,
        MagDump = 0x01,
        TimeSync = 0x02,
        DebugMessage = 0x03,
        BnoDump = 0x04,
        BmpDump = 0x05
    }
}