using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemDistributor 
{
    // 아이템 종류와 목표 "개수" 혹은 "가중치" 설정용
    [Serializable]
    public struct ItemWeight
    {
        public Define.ItemInfomation item;
        public int weight; // 비율/가중치 (총량을 비율로부터 계산)
    }

    public static void DistributeItemsFairly(
        TileMapManager tileMapManager,
        int width, int height,
        Vector2Int[] spawnPoints,          // 필수
        ItemWeight[] weights,              // 필수
        int safeRadiusFromSpawn = 2,       // ---- 여기서부터 옵셔널 시작
        float fillRatio = 0.35f,
        int? fixedTotalCount = null,
        int? seed = null,
        bool useSectors = false,
        int sectorRows = 2, int sectorCols = 2
    )
    {
        System.Random rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();

        // 1) 후보 수집: "부술 수 있는 벽"만 아이템 가능하다고 가정
        List<Vector2Int> candidates = new List<Vector2Int>(width * height);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                if (tileMapManager.tileMapInfos[y, x] == null) continue;
                if (tileMapManager.tileMapInfos[y, x].TileMapInfomation != Define.TileMapInfomation.WALL) continue; // Soft wall만
                                                                                                     // 스폰 근처 보호
                if (IsNearAnySpawn(new Vector2Int(x, y), spawnPoints, safeRadiusFromSpawn)) continue;
                candidates.Add(new Vector2Int(x, y));
            }

        // 셀 셔플
        Shuffle(candidates, rng);

        int targetSlots = fixedTotalCount ?? Mathf.RoundToInt(candidates.Count * fillRatio);
        targetSlots = Mathf.Clamp(targetSlots, 0, candidates.Count);

        // 2) 아이템 봉지 만들기 (비율→개수)
        List<Define.ItemInfomation> bag = MakeItemBag(weights, targetSlots, rng);

        if (!useSectors)
        {
            // 3) 셔플된 후보 앞에서부터 배정
            int n = Math.Min(bag.Count, candidates.Count);
            for (int i = 0; i < n; i++)
            {
                var p = candidates[i];
                tileMapManager.tileMapInfos[p.y, p.x].itemInfomation = bag[i];
            }
        }
        else
        {
            // 3’) 섹터별 분배
            var sectors = SplitIntoSectors(candidates, width, height, sectorRows, sectorCols);
            // 섹터별 목표 개수 계산 (균등 혹은 후보수 비례)
            int totalCandidates = candidates.Count;
            int consumed = 0;
            int bagIdx = 0;

            for (int s = 0; s < sectors.Count; s++)
            {
                var sector = sectors[s];
                // 후보수 비례로 섹터 타겟
                int sectorTarget = Mathf.RoundToInt((sector.Count / (float)totalCandidates) * bag.Count);
                sectorTarget = Math.Min(sectorTarget, bag.Count - consumed);

                // 섹터 셀 셔플
                Shuffle(sector, rng);

                int take = Math.Min(sectorTarget, sector.Count);
                for (int i = 0; i < take; i++)
                {
                    var p = sector[i];
                    tileMapManager.tileMapInfos[p.y, p.x].itemInfomation = bag[bagIdx++];
                }
                consumed += take;
            }

            // 남은 게 있으면(라운딩 오차) 여분 섹터에 추가 배치
            int remain = bag.Count - consumed;
            if (remain > 0)
            {
                // 다시 전체 후보에서 아직 비어있는 칸에 채우기
                foreach (var p in candidates)
                {
                    if (remain <= 0) break;
                    if (tileMapManager.tileMapInfos[p.y, p.x].itemInfomation == Define.ItemInfomation.None)
                    {
                        tileMapManager.tileMapInfos[p.y, p.x].itemInfomation = bag[bagIdx++];
                        remain--;
                    }
                }
            }
        }
    }

    static bool IsNearAnySpawn(Vector2Int cell, Vector2Int[] spawns, int radius)
    {
        if (spawns == null) return false;
        foreach (var s in spawns)
        {
            int dist = Mathf.Abs(s.x - cell.x) + Mathf.Abs(s.y - cell.y); // Manhattan
            if (dist <= radius) return true;
        }
        return false;
    }

    static List<Define.ItemInfomation> MakeItemBag(ItemWeight[] weights, int total, System.Random rng)
    {
        int sum = 0;
        foreach (var w in weights) sum += Math.Max(0, w.weight);
        sum = Math.Max(1, sum);

        // 임시 비율 할당
        var provisional = new List<(Define.ItemInfomation item, int count)>();
        int assigned = 0;
        foreach (var w in weights)
        {
            int c = Mathf.RoundToInt(total * (w.weight / (float)sum));
            provisional.Add((w.item, c));
            assigned += c;
        }

        // 라운딩 보정
        int diff = total - assigned;

        if (diff != 0)
        {
            // 가중치 내림차순 인덱스
            var order = new List<int>();
            for (int i = 0; i < weights.Length; i++) order.Add(i);
            order.Sort((a, b) => weights[b].weight.CompareTo(weights[a].weight)); // desc

            // 증가 보정 (diff > 0)
            int idx = 0;
            while (diff > 0 && order.Count > 0)
            {
                int iOrd = order[idx % order.Count];
                var cur = provisional[iOrd];
                // Unity/C# 버전에 따라 아래 둘 중 하나 사용:
                // provisional[iOrd] = (cur.item, cur.count + 1);   // 이름 보존될 때
                provisional[iOrd] = (cur.Item1, cur.Item2 + 1);     // 이름 미보존 대비 안전
                diff--; idx++;
            }

            // 감소 보정 (diff < 0) → 가중치 오름차순으로 줄이기
            order.Reverse();
            idx = 0;
            while (diff < 0 && order.Count > 0)
            {
                int iOrd = order[idx % order.Count];
                var cur = provisional[iOrd];
                int newCount = Math.Max(0, /*cur.count*/ cur.Item2 - 1);
                // provisional[iOrd] = (cur.item, newCount);
                provisional[iOrd] = (cur.Item1, newCount);
                diff++; idx++;
            }
        }

        // 봉지 만들고 셔플
        var bag = new List<Define.ItemInfomation>(total);
        foreach (var p in provisional)
        {
            // int c = p.count;
            int c = p.Item2;
            for (int i = 0; i < c; i++) bag.Add(/*p.item*/ p.Item1);
        }
        Shuffle(bag, rng);
        return bag;
    }


    static List<List<Vector2Int>> SplitIntoSectors(List<Vector2Int> candidates, int width, int height, int rows, int cols)
    {
        var sectors = new List<List<Vector2Int>>(rows * cols);
        for (int i = 0; i < rows * cols; i++) sectors.Add(new List<Vector2Int>());

        int cellW = Mathf.CeilToInt(width / (float)cols);
        int cellH = Mathf.CeilToInt(height / (float)rows);

        foreach (var p in candidates)
        {
            int cx = Mathf.Clamp(p.x / cellW, 0, cols - 1);
            int cy = Mathf.Clamp(p.y / cellH, 0, rows - 1);
            int idx = cy * cols + cx;
            sectors[idx].Add(p);
        }
        return sectors;
    }

    static void Shuffle<T>(IList<T> list, System.Random rng)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
