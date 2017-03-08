namespace DemoSatSpring2017Netduino_OnboardSD.Work_Items {
    public enum PacketType : byte {
        StartByte = 0xFF,
        FMagDump = 0x00,
        BmpDump = 0x01,
        BnoDump = 0x02,
        TimeSync = 0x03,
        DebugMessage = 0x04,
        EMagDump = 0x05
    }
}