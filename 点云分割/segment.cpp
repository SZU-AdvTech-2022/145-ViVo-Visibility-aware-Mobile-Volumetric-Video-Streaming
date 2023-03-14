#include<iostream>
#include<pcl/visualization/cloud_viewer.h>
#include<pcl/point_cloud.h>
#include<pcl/io/pcd_io.h>
#include<pcl/io/ply_io.h>
#include<string>
#include <pcl/point_types.h>
#include <pcl/compression/octree_pointcloud_compression.h>

#include<windows.h>


#include <io.h>
#include <direct.h>
#include <string>
#include <vector>

using namespace std;

string point_data = "E:/Download/ply���ݼ�/longdress/new_compress/point_num.txt";
string point_data_b = "E:/Download/ply���ݼ�/longdress/new_compress/point_num_b.txt";

void getFiles(std::string path, std::vector<std::string>& files);

void Split(string filename, string temp, int count);

void Compress(pcl::PointCloud<pcl::PointXYZRGB>::Ptr cloud, string f, int i);

int main()
{
	// ��ȡ�ļ��б�
	vector <string> files;   // ����ļ�·��������
	string filePath = "E:/Download/ply���ݼ�/longdress/ply";

	getFiles(filePath, files);

	int count = 0;
	string temp;
	// ��ȡ�ļ��������зָ

	for (string plyfile : files)
	{
		// ÿ5��֡��һ���ļ��У�temp��tile�ı���·��
		temp = "E:/Download/ply���ݼ�/longdress/new_compress/" + to_string(count + 1);
		if (_access(temp.c_str(), 0) == -1)	//����ļ��в�����
			_mkdir(temp.c_str());
		Split(plyfile, temp, count);
		count++;	
	}

	return 0;
}

// ��������
void getFiles(std::string path, std::vector<std::string>& files)
{
	intptr_t   hFile = 0;	//intptr_t��uintptr_t������:typedef long int�� typedef unsigned long int
	struct _finddata_t fileinfo;
	std::string p;
	if ((hFile = _findfirst(p.assign(path).append("/*").c_str(), &fileinfo)) != -1)//assign�������Ƚ�ԭ�ַ�����գ�Ȼ�����µ�ֵ���滻��
	{
		do
		{
			if (strcmp(fileinfo.name, ".") != 0 && strcmp(fileinfo.name, "..") != 0)
			{
				files.push_back(p.assign(path).append("/").append(fileinfo.name));
			}
		} while (_findnext(hFile, &fileinfo) == 0);
		_findclose(hFile);
	}
}


void Split(string filename, string temp, int count)
{
	cout << filename << endl;

	pcl::PointCloud<pcl::PointXYZRGB>::Ptr cloud(new pcl::PointCloud<pcl::PointXYZRGB>);
	pcl::io::loadPLYFile<pcl::PointXYZRGB>(filename, *cloud);

	pcl::PointCloud<pcl::PointXYZRGB>::Ptr newcloud(new pcl::PointCloud<pcl::PointXYZRGB>);

	// ��һ��,���� (20, 0, 0)  ---> Ŀ��ռ� 12*12*12
	for (long i = 0; i < cloud->size(); i++)
	{
		cloud->points[i].x = cloud->points[i].x /1024 *12;
		cloud->points[i].y = cloud->points[i].y /1024 *12 ;
		cloud->points[i].z = cloud->points[i].z /1024 *12 ;
	}
	// �ָ�
	for (int x = 0; x < 6; x++)
	{
		for (int y = 0; y < 6; y++)
		{
			for (int z = 0; z < 6; z++)
			{
				for (long i = 0; i < cloud->size(); i++)
				{
					if (x * 2 < cloud->points[i].x && cloud->points[i].x < (x+1)*2 && \
						y * 2 < cloud->points[i].y && cloud->points[i].y < (y+1)*2 && \
						z * 2 < cloud->points[i].z && cloud->points[i].z < (z+1)*2 )
					{

						newcloud->push_back(cloud->points[i]);
					}
				}
				// �ļ�����֡id_xyz.ply xyz��tile���п������ꡣ
				if (newcloud->size() == 0)
				{
					cout << " this tile have no point" << endl;
				}
				else
				{
					string f = temp + "/" + to_string(count + 1) + "_" + to_string(x + 1) + to_string(y + 1) + to_string(z + 1);
					for (int i = 0; i < 5; i++)
					{
						Compress(newcloud, f, i);
					}
					
				}		
				// ά��һ������tile���������ļ�
				if (count % 5 == 0)
				{
					fstream file_1;
					fstream file_2;
					file_1.open(point_data, std::ios_base::app | std::ios_base::in);
					file_2.open(point_data_b, std::ios::binary|std::ios_base::app | std::ios_base::in);
					if (file_1.is_open())
					{
						file_1 << newcloud->size() << "\t" ;
					}
					if (file_2.is_open())
					{
						int temp = newcloud->size();
						file_2.write(reinterpret_cast<char *>(&temp), sizeof(int));
					}
					file_1.close();
					file_2.close();
				}

				newcloud.reset(new pcl::PointCloud<pcl::PointXYZRGB>);
			}
		}

	}
}


void Compress(pcl::PointCloud<pcl::PointXYZRGB>::Ptr cloud, string f, int level)
{
#pragma region Compress
	double compressLevel[] = {0.05, 0.027, 0.021, 0.015, 0.013, 1 }; // 1����ѹ��
	bool showStatistics = true;
	pcl::io::compression_Profiles_e compressionProfile = pcl::io::MANUAL_CONFIGURATION;
	pcl::io::OctreePointCloudCompression<pcl::PointXYZRGB>* PointCloudEncoder;

	std::stringstream compressedData;
	pcl::PointCloud<pcl::PointXYZRGB>::Ptr cloudOut(new pcl::PointCloud<pcl::PointXYZRGB>());

	if (level != 4)
	{
		PointCloudEncoder = new pcl::io::OctreePointCloudCompression<pcl::PointXYZRGB>(compressionProfile, showStatistics, 0.001, compressLevel[level], true, 100, true, 8);//�������
	/// <summary>
	/// �˲���������-----------�����²��������------------ѹ����
	///    0.02						27100				0.376
	///    0.015					46062				0.64
	///	   0.013					59702				0.8
	/// </summary>
	/// <returns></returns>

		PointCloudEncoder->encodePointCloud(cloud, compressedData);
		PointCloudEncoder->decodePointCloud(compressedData, cloudOut);
	}
	else {
		cloudOut = cloud;
	}
#pragma endregion



#pragma region writePlyfile
	string temp = "";
	switch (level)
	{
	case 0:
		temp = "_20.ply"; break;
	case 1:
		temp = "_40.ply"; break;
	case 2:
		temp = "_60.ply"; break;
	case 3:
		temp = "_80.ply"; break;
	case 4:
		temp = "_100.ply"; break;
	default:
		cout << "error compressLevel" << endl; break;
	}
	f += temp;
	fstream pointFile;
	pointFile.open(f, ios_base::out | ios::binary);
	if (pointFile.is_open())
	{
#pragma region header
		pointFile << "ply" << endl;
		pointFile << "format binary_little_endian 1.0" << endl;
		pointFile << "comment Generated by iPi Motion Capture" << endl;
		pointFile << "element vertex " << cloudOut->size() << endl;
		pointFile << "property float x" << endl;
		pointFile << "property float y" << endl;
		pointFile << "property float z" << endl;
		pointFile << "property uchar red" << endl;
		pointFile << "property uchar green" << endl;
		pointFile << "property uchar blue" << endl;
		pointFile << "end_header" << endl;
#pragma endregion

		for (long i = 0; i < cloudOut->size(); i++)
		{
			pointFile.write(reinterpret_cast<char*>(&cloudOut->points[i].x), sizeof(cloudOut->points[i].x));
			pointFile.write(reinterpret_cast<char*>(&cloudOut->points[i].y), sizeof(cloudOut->points[i].y));
			pointFile.write(reinterpret_cast<char*>(&cloudOut->points[i].z), sizeof(cloudOut->points[i].z));
			pointFile.write(reinterpret_cast<char*>(&cloudOut->points[i].r), sizeof(cloudOut->points[i].r));
			pointFile.write(reinterpret_cast<char*>(&cloudOut->points[i].g), sizeof(cloudOut->points[i].g));
			pointFile.write(reinterpret_cast<char*>(&cloudOut->points[i].b), sizeof(cloudOut->points[i].b));
		}

	}

	pointFile.close();
#pragma endregion

}