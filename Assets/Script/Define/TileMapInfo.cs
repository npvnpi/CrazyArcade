using UnityEngine;

public class TileMapInfo 
{
    public Define.TileMapInfomation TileMapInfomation { get; set; }

    public TileMapInfo(Define.TileMapInfomation tileMapInfo) { this.TileMapInfomation = tileMapInfo; }

    public Define.ItemInfomation itemInfomation { get; set; }

}
