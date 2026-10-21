public enum MagicType
{
    Enchant,
    Ensure,
    CancelEnchant
}
public enum SymbolType
{
    Preview, Confirm
}
public enum OutShapeType
{
    Hexagon,
    Pentagon,
    Square,
    None,
    Circle, // RangePolygonDrawer에서만 씀
}
public enum RequestType
{
    아이편지, 이쁜편지, 귀족편지, 일반편지, 현상수배, 그림편지, 제국공고, 주문서, 메모, 일반공고, 비밀메모
}

public enum ReplyType
{
    주문서, 편지, 공고, 메모
}

// 값은 0부터 연속이어야 함 (의뢰 데이터 Content_{이름} 컬럼, 로케일 인덱스, Resources 폴더 이름에 사용)
public enum Language
{
    Kor = 0,
    Eng = 1,
    Jp = 2,
}