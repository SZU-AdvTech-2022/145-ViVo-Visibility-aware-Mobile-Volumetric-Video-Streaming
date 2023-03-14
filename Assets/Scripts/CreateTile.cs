using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using System;
using System.IO;

using System.Linq;

//using Math;

struct Tile_level
{
    public Bounds bound;   // ���tile�ı߿�
    public int level;      // ��ŵȼ�
    public Tile_level(Bounds j, int i)
    {
        bound = j;
        level = i;    }
}

public class CreateTile
{
    private List<GameObject> obj_list;       // �����Ŀ���б�
    private GameObject root;                 // ������ĸ�Ŀ¼

    private int tile_sum;                    //һ��ά���Ϸֿ������
    private float tile_size;                 // ��Ĵ�С
    private int initial_level;               // ��ʼ�����ȼ�

    private Plane[] planes;                  // ��׶ƽ��
    private List<Tile_level> pv_result;      // Ԥ��ɼ��ԵĽ��
    private List<Tile_level> ov_result;      // �ڵ��ɼ��ԵĽ��
    private List<Tile_level> dv_result;      // ��ȿɼ��ԵĽ��


    public CreateTile()
    {
        tile_sum = 4;
        tile_size = 12 / tile_sum;
        initial_level = 3;

        planes = new Plane[6];
        obj_list = new List<GameObject>();

        pv_result = new List<Tile_level>();
        ov_result = new List<Tile_level>();
        dv_result = new List<Tile_level>();
        
        root = new GameObject("Root");

        // �����п鷽��������Ӧ��cube
        Vector3 offset = new Vector3(tile_size/2, tile_size/2, tile_size/2);
        for (var i = 0; i < tile_sum; i++)
        {
            for (var j = 0; j < tile_sum; j++)
            {
                for (var k = 0; k < tile_sum; k++)
                {
                    var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    go.transform.position = new Vector3(i, j, k) * tile_size + offset;
                    go.transform.localScale = new Vector3(tile_size, tile_size, tile_size);
                    go.transform.parent = root.transform;
                    go.GetComponent<Renderer>().enabled = false;
                    obj_list.Add(go);
                }
            }
        }
    }

    public void PV(GameObject cam)
    {
        pv_result.Clear();
        for (int i = 1; i < 4; i++)
        {
            cam.GetComponent<Camera>().fieldOfView = 30 * i;
            GeometryUtility.CalculateFrustumPlanes(cam.GetComponent<Camera>(), planes);
            for (var index = 0; index < obj_list.Count; index++)
            {
                var bounds = obj_list[index].GetComponent<Renderer>().bounds;
                var result = GeometryUtility.TestPlanesAABB(planes, bounds);    // �ж��Ƿ�����׶��
                Tile_level temp = new Tile_level(bounds, initial_level - i);
                if (result && !pv_result.Exists(t => t.bound == temp.bound))
                {
                    pv_result.Add(temp);
                }
            }
        }
    }

    #region ʵ��

    // ����һ��ʵ���÷���
    // ����ʵ����������Ŀ��Ԥ����������Ŀ��¼�������ļ��С�������ȡ�ļ���֤׼ȷ��
    //public void write_file(GameObject actul_cam, GameObject pre_cam)
    //{
    //    string pre_file = "F:/Ԥ��.txt";
    //    string actul_file = "F:/ʵ��.txt";

    //    GeometryUtility.CalculateFrustumPlanes(actul_cam.GetComponent<Camera>(), planes);
    //    string time_now = DateTime.Now.ToString("HH:mm:ss:fff");
    //    File.AppendAllText(actul_file, time_now + " ");
    //    for (int x = 0; x < tile_sum; x++)
    //    {
    //        for (int y = 0; y < tile_sum; y++)
    //        {
    //            for (int z = 0; z < tile_sum; z++)
    //            {
    //                int index = x * tile_sum * tile_sum + y * tile_sum + z;
    //                var bounds = obj_list[index].GetComponent<Renderer>().bounds;
    //                var result = GeometryUtility.TestPlanesAABB(planes, bounds);
    //                if (result)
    //                {
    //                    File.AppendAllText(actul_file, "1 ");
    //                }
    //                else
    //                {
    //                    File.AppendAllText(actul_file, "0 ");
    //                }
    //            }
    //        }
    //    }
    //    File.AppendAllText(actul_file, Environment.NewLine);

    //    GeometryUtility.CalculateFrustumPlanes(pre_cam.GetComponent<Camera>(), planes);
    //    File.AppendAllText(pre_file, time_now + " ");
    //    for (int x = 0; x < tile_sum; x++)
    //    {
    //        for (int y = 0; y < tile_sum; y++)
    //        {
    //            for (int z = 0; z < tile_sum; z++)
    //            {
    //                int index = x * tile_sum * tile_sum + y * tile_sum + z;
    //                var bounds = obj_list[index].GetComponent<Renderer>().bounds;
    //                var result = GeometryUtility.TestPlanesAABB(planes, bounds);
    //                if (result)
    //                {
    //                    File.AppendAllText(pre_file, "1 ");
    //                }
    //                else
    //                {
    //                    File.AppendAllText(pre_file, "0 ");
    //                }
    //            }
    //        }
    //    }
    //    File.AppendAllText(pre_file, Environment.NewLine);
    //}

    #endregion


    public void OV(GameObject cam)
    {
        ov_result.Clear();
        //================��ȡ�ʼ�==================
        string path = "F:/PLY/point_num_b.txt";         //��ŵ��������ļ�
        int tile_len = (int)(Math.Pow(tile_sum, 3));
        int[] point_number = new int[tile_len];
        BinaryReader br;
        br = new BinaryReader(new FileStream(path, FileMode.Open));
        for (int i = 0; i < point_number.Length; i++)
        {
            point_number[i] = br.ReadInt32();
        }
        br.Close();

        // �㷨�е� �� 0��1��2 �� ��
        double[] parament = new double[4] { 0.6, 1, 3, 0.8};

        for (var index = 0; index < pv_result.Count; index++)
        {
            var bound_temp = pv_result[index].bound;

            // ��ȡ��ĵ������
            Vector3 tile_position = (bound_temp.center - new Vector3(tile_size / 2, tile_size / 2, tile_size / 2)) / tile_size;
            int tile_point = point_number[((int)(tile_position.x * tile_sum * tile_sum + tile_position.y * tile_sum + tile_position.z))];
            if(tile_point == 0)
            {
                continue;
            }

            RaycastHit[] hits;
            hits = Physics.RaycastAll(bound_temp.center, cam.transform.position - bound_temp.center, tile_size);
            int s = hits.Length;                        //�㷨�е� S(c)
            int max_point = 0;

            for(int i =0;i<s; i++)
            {
                Vector3 temp_ov = (hits[i].transform.position - new Vector3(tile_size / 2, tile_size / 2, tile_size / 2)) / tile_size;
                int k = point_number[((int)(temp_ov.x * tile_sum * tile_sum + temp_ov.y * tile_sum + temp_ov.z))];
                if (k > max_point)
                {
                    max_point = k;
                }
            }
            float r = max_point / tile_point;         // �㷨�е� R(c)
            // �����ڵ������Ե����ȼ�         
            double temp = Math.Pow(parament[3], s - 1);
            int d_level = 0;
            if (r < parament[0] * temp)
            {
                d_level = 0;
            }
            else if(r< parament[1] * temp)
            {
                d_level = 1;
            }
            else if(r< parament[2] * temp)
            {
                d_level = 2;
            }
            else
            {
                d_level = 3;
            }
            Tile_level ov_res = new Tile_level(pv_result[index].bound, pv_result[index].level - d_level);
            ov_result.Add(ov_res);
        }
    }

    public void DV(GameObject cam)
    {
        dv_result.Clear();
        ov_result = ov_result.OrderByDescending(t => t.level).ToList();                       // �������ȼ��������С�
        float[] distance_threshold =new float[]{ 19.2f, 25.2f, 31.2f, 37.2f };             // ����5���������ֵ��ע���ʵ�ʿռ䵽unity�ռ��ӳ�� 
        for (var index = 0; index < ov_result.Count; index++)
        {
            if(ov_result[index].level <= 0)
            {
                break;
            }
            var bound_temp = ov_result[index].bound;
            float dis1 = (cam.transform.position - bound_temp.center).magnitude;

            // ���ݿ鵽 ����ľ��� dis1,ѡ����Ӧ��ѹ���̶�
            // ���յ�tile���ȼ�һ����Ϊ 5�����ֱ��Ӧ��5��ѹ���̶ȡ�
            if(dis1 < distance_threshold[0])
            {
                dv_result.Add(new Tile_level(ov_result[index].bound, 5));
            }
            else if(dis1 < distance_threshold[1])
            {
                dv_result.Add(new Tile_level(ov_result[index].bound, 4));
            }
            else if(dis1 < distance_threshold[2])
            {
                dv_result.Add(new Tile_level(ov_result[index].bound, 3));
            }
            else if(dis1 <distance_threshold[3])
            {
                dv_result.Add(new Tile_level(ov_result[index].bound, 2));
            }
            else
            {
                dv_result.Add(new Tile_level(ov_result[index].bound, 1));
            }
        }
    }

    public void Get_tile_number(List<string> tile_number)
    {
        tile_number.Clear();
        for (var index = 0; index < dv_result.Count; index++)
        {
            if (dv_result[index].level > 0)
            {
                int level_temp = dv_result[index].level * 20;
                var bound_temp = dv_result[index].bound;
                // tile �ı���� tile_position�� xyz ���ꡣ�����ļ��������ڲ����������ļ��б�ʹ�á�ע�������ƫ��
                Vector3 tile_position = (bound_temp.center + new Vector3(tile_size / 2, tile_size / 2, tile_size / 2)) / tile_size;
                string tile_name = tile_position.x.ToString("0") +
                                   tile_position.y.ToString("0") +
                                   tile_position.z.ToString("0") +
                                   "_" + level_temp.ToString() + ".ply";
                tile_number.Add(tile_name);
            }
        }
    }
}
