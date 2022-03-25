// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!
// 姓名：
// 学号：
// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!
// bonus部分，注意这个scene中character的rest pose和bvh文件中的rest pose不同，两只手从水平变成斜向下45度，需要对数据做retarget
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ! 在以下部分定义需要的类或结构体


// ! 在以上空白写下你的代码

public class BVHLoader2 : MonoBehaviour
{
    private string bvh_fname = "Assets\\BVH\\cartwheel.bvh";
    // 从bvh文件中读取所有的关节名称，需要用关节名称来找unity scene中的game object
    private List<string> joints = new List<string>();
    // game object列表
    private List<GameObject> gameObjects = new List<GameObject>();
    // 时间戳
    private int time_step = 0;
    // bvh的帧数
    private int frame_num = 0;
    string tmp_name;
    // ! 在这里声明你需要的其他数据结构
    private float frame_time = 0f;
    public bool IsAlpha(string input)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(input, "^[a-zA-Z]+$");
    }
    public List<JOINT> joint_ls = new List<JOINT>();
    Stack st = new Stack();
    int col_num = 0;
    // ! 在以上空白写下你的代码
    public int left_b = -1;
    public int row_num = 0;
    float[,] mat = new float[2111, 78];
    Dictionary<string,int> name2idx = new Dictionary<string, int>();
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        StreamReader bvh_file = new StreamReader(new FileStream(bvh_fname, FileMode.Open));
        while (!bvh_file.EndOfStream)
        {
            //JOINT tmp_joint = new JOINT();
            string line = bvh_file.ReadLine();
            string str = new System.Text.RegularExpressions.Regex("[\\s]+").Replace(line, " ");
            //print("str= " + str);
            string[] split_line = str.Split(' ');
            //print("splitstr= " + split_line);
            // 处理bvh文件中的character hierarchy部分
            if (line.Contains("ROOT") || line.Contains("JOINT"))
            {
                // ! 处理这一行的信息
                // add names to joints
                foreach (string i in split_line)
                    if (i != "ROOT" && i != "JOINT" && (IsAlpha(i) || i.Contains("_")))
                    {
                        tmp_name = i;
                        joints.Add(i);
                    }


                if (!line.Contains("ROOT"))
                {   //print("left: " + left_b);
                    // print("right: "+ right_b);
                    //print("parent is: " + (left_b - right_b));
                    // print("count: "+joint_ls.Count);
                    // print(left_b-right_b);



                    int top = (int)st.Peek();
                    joint_ls.Add(new JOINT(joints[joints.Count - 1], top));
                    //print(joint_ls[joint_ls.Count-1].joint_name+" setting parent to: " + joint_ls[top].joint_name);


                }
                else
                {
                    joint_ls.Add(new JOINT("RootJoint"));
                }
                // ! 在以上空白写下你的代码
            }
            else if (line.Contains("End Site"))
            {
                // ! 处理这一行的信息
                int top = (int)st.Peek();
                // print("left: " + left_b);
                // print("right: "+ right_b);
                joints.Add(joints[joints.Count - 1] + "_end");
                joint_ls.Add(new JOINT(joints[joints.Count - 1], top));
                // ! 在以上空白写下你的代码
            }
            else if (line.Contains("{"))
            {
                // ! 处理这一行的信息
                left_b++;
                st.Push(left_b);
                // ! 在以上空白写下你的代码
            }
            else if (line.Contains("}"))
            {
                // ! 处理这一行的信息
                //right_b++;
                st.Pop();
                // ! 在以上空白写下你的代码
            }
            else if (line.Contains("OFFSET"))
            {
                // ! 处理这一行的信息
                float x = (float)System.Convert.ToDouble(split_line[2]);
                float y = (float)System.Convert.ToDouble(split_line[3]);
                float z = (float)System.Convert.ToDouble(split_line[4]);
                joint_ls[joint_ls.Count - 1].offset_x = x;
                joint_ls[joint_ls.Count - 1].offset_y = y;
                joint_ls[joint_ls.Count - 1].offset_z = z;
                // ! 在以上空白写下你的代码
            }
            else if (line.Contains("CHANNELS"))
            {
                // ! 处理这一行的信息

                // ! 在以上空白写下你的代码
            }
            else if (line.Contains("Frame Time"))
            {
                // ! 处理这一行的信息
                frame_time = float.Parse(split_line[split_line.Length - 1]);
                //print(frame_time);

                // ! 在以上空白写下你的代码
                // Frame Time是数据部分前的最后一行，读到这一行后跳出循环
                break;
            }
            else if (line.Contains("Frames:"))
            {
                // ! 处理这一行的信息


                // ! 在以上空白写下你的代码
                // 获取帧数
                frame_num = int.Parse(split_line[split_line.Length - 1]);

            }

        }


        // 接下来处理bvh文件中的数据部分
        while (!bvh_file.EndOfStream)
        {
            string line = bvh_file.ReadLine();
            string str = new System.Text.RegularExpressions.Regex("[\\s]+").Replace(line, " ");
            string[] split_line = str.Split(' ');
            // ! 解析每一行数据，保存在合适的数据结构中，用于之后update
            // 注意数据的顺序是和之前的channel顺序对应的
            // 提示：欧拉角顺序可能有多种，但都可以用三个四元数相乘得到，注意相乘的顺序
            col_num = joint_ls.Count * 3 + 3;


            for (int i = 1; i < split_line.Length; i++)
            {
                float elm = float.Parse(split_line[i]);
                mat[row_num, i - 1] = elm;
            }

            row_num++;

            // ! 在以上空白写下你的代码
        }

        // 按关节名称获取所有的game object
        GameObject tmp_obj = new GameObject();
        for (int i = 0; i < joints.Count; i++)
        {
            tmp_obj = GameObject.Find(joints[i]);
            //print(joints[i]);
            gameObjects.Add(tmp_obj);
        }
        for (int i = 0; i < joints.Count; i++)
        {
            
            print(gameObjects[i].name);
        }
        // ! 在这里写下你认为需要的其他操作
        for(int i = 0,cnt=0;i < joints.Capacity;i++){
            if(!joints[i].Contains("end")){
                name2idx[joints[i]] = cnt++; 
            }
        }
        // ! 在以上空白写下你的代码
    }

    // Update is called once per frame
    void Update()
    {

        Vector3 joint_position = new Vector3(0.0F, 0.0F, 0.0F);
        Quaternion joint_orientation = Quaternion.identity;
        // ! 定义你需要的局部变量
        int frame_cnt = time_step;
        // ! 在以上空白写下你的代码

        for (int i = 0; i < joints.Count; i++)
        {
            
            // ! 进行前向运动学的计算，根据之前解析出的每一帧局部位置、旋转获得每个关节的全局位置、旋转
            int parent_idx = joint_ls[i].parent;
            if (i == 0)
            {
                //Pos
                Vector3 current_offset = new Vector3(mat[frame_cnt, 0], mat[frame_cnt, 1], mat[frame_cnt, 2]);
                Vector3 current_rot = new Vector3(mat[frame_cnt, 4], mat[frame_cnt, 3], mat[frame_cnt, 5]);
                joint_position = current_offset;
                joint_orientation = Quaternion.Euler(current_rot);
                //Rot

            }
            else
            {
                
                Vector3 current_offset = new Vector3(joint_ls[i].offset_x, joint_ls[i].offset_y, joint_ls[i].offset_z);
                
                
                joint_position = gameObjects[parent_idx].transform.position + gameObjects[parent_idx].transform.rotation * current_offset;
                if(joints[i].Contains("end")){
                    joint_orientation = Quaternion.identity;
                }
                else
                {
                    Vector3 current_rot = new Vector3(mat[frame_cnt, (name2idx[joints[i]] - 1) * 3 + 7], mat[frame_cnt, (name2idx[joints[i]] - 1) * 3 + 6], mat[frame_cnt, (name2idx[joints[i]] - 1) * 3 + 8]);
                    joint_orientation = Quaternion.Euler(current_rot) * gameObjects[parent_idx].transform.rotation;
                }


            }


            // ! 在以上空白写下你的代码

            // 更新每个关节的全局位置和旋转

            gameObjects[i].transform.position = joint_position;
            gameObjects[i].transform.rotation = joint_orientation;

        }
        // print(frame_cnt);
        // 更新时间戳

        time_step = (time_step + 1) % frame_num;
    }
}
