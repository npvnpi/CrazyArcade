using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemDistributor 
{
    // ������ ������ ��ǥ "����" Ȥ�� "����ġ" ������
    [Serializable]
    public struct ItemWeight
    {
        public Define.ItemInfomation item;
        public int weight; // ����/����ġ (�ѷ��� �����κ��� ���)
    }

    public static void DistributeItemsFairly(
        TileMapManager tileMapManager,
        int width, int height,
        Vector2Int[] spawnPoints,          // �ʼ�
        ItemWeight[] weights,              // �ʼ�
        int safeRadiusFromSpawn = 2,       // ---- ���⼭���� �ɼų� ����
        float fillRatio = 0.35f,
        int? fixedTotalCount = null,
        int? seed = null,
        bool useSectors = false,
        int sectorRows = 2, int sectorCols = 2
    )
    {
        System.Random rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();

        // 1) �ĺ� ����: "�μ� �� �ִ� ��"�� ������ �����ϴٰ� ����
        List<Vector2Int> candidates = new List<Vector2Int>(width * height);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                if (tileMapManager.tileMapInfos[y, x] == null) continue;
                if (tileMapManager.tileMapInfos[y, x].TileMapInfomation != Define.TileMapInfomation.WALL) continue; // Soft wall��
                                                                                                     // ���� ��ó ��ȣ
                if (IsNearAnySpawn(new Vector2Int(x, y), spawnPoints, safeRadiusFromSpawn)) continue;
                candidates.Add(new Vector2Int(x, y));
            }

        // �� ����
        Shuffle(candidates, rng);

        int targetSlots = fixedTotalCount ?? Mathf.RoundToInt(candidates.Count * fillRatio);
        targetSlots = Mathf.Clamp(targetSlots, 0, candidates.Count);

        // 2) ������ ���� ����� (�����氳��)
        List<Define.ItemInfomation> bag = MakeItemBag(weights, targetSlots, rng);

        if (!useSectors)
        {
            // 3) ���õ� �ĺ� �տ������� ����
            int n = Math.Min(bag.Count, candidates.Count);
            for (int i = 0; i < n; i++)
            {
                var p = candidates[i];
                tileMapManager.tileMapInfos[p.y, p.x].itemInfomation = bag[i];
            }
        }
        else
        {
            // 3��) ���ͺ� �й�
            var sectors = SplitIntoSectors(candidates, width, height, sectorRows, sectorCols);
            // ���ͺ� ��ǥ ���� ��� (�յ� Ȥ�� �ĺ��� ���)
            int totalCandidates = candidates.Count;
            int consumed = 0;
            int bagIdx = 0;

            for (int s = 0; s < sectors.Count; s++)
            {
                var sector = sectors[s];
                // �ĺ��� ��ʷ� ���� Ÿ��
                int sectorTarget = Mathf.RoundToInt((sector.Count / (float)totalCandidates) * bag.Count);
                sectorTarget = Math.Min(sectorTarget, bag.Count - consumed);

                // ���� �� ����
                Shuffle(sector, rng);

                int take = Math.Min(sectorTarget, sector.Count);
                for (int i = 0; i < take; i++)
                {
                    var p = sector[i];
                    tileMapManager.tileMapInfos[p.y, p.x].itemInfomation = bag[bagIdx++];
                }
                consumed += take;
            }

            // ���� �� ������(���� ����) ���� ���Ϳ� �߰� ��ġ
            int remain = bag.Count - consumed;
            if (remain > 0)
            {
                // �ٽ� ��ü �ĺ����� ���� ����ִ� ĭ�� ä���
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

        // �ӽ� ���� �Ҵ�
        var provisional = new List<(Define.ItemInfomation item, int count)>();
        int assigned = 0;
        foreach (var w in weights)
        {
            int c = Mathf.RoundToInt(total * (w.weight / (float)sum));
            provisional.Add((w.item, c));
            assigned += c;
        }

        // ���� ����
        int diff = total - assigned;

        if (diff != 0)
        {
            // ����ġ �������� �ε���
            var order = new List<int>();
            for (int i = 0; i < weights.Length; i++) order.Add(i);
            order.Sort((a, b) => weights[b].weight.CompareTo(weights[a].weight)); // desc

            // ���� ���� (diff > 0)
            int idx = 0;
            while (diff > 0 && order.Count > 0)
            {
                int iOrd = order[idx % order.Count];
                var cur = provisional[iOrd];
                // Unity/C# ������ ���� �Ʒ� �� �� �ϳ� ���:
                // provisional[iOrd] = (cur.item, cur.count + 1);   // �̸� ������ ��
                provisional[iOrd] = (cur.Item1, cur.Item2 + 1);     // �̸� �̺��� ��� ����
                diff--; idx++;
            }

            // ���� ���� (diff < 0) �� ����ġ ������������ ���̱�
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

        // ���� ����� ����
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
