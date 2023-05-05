/**
 * Danceology
 * Originally Developed by Team Danceology Spring 2023
 * Christine Jung, Xiaoying Meng, Jiacheng Qiu, Yiming Xiao, Xueying Yang, Angela Zhang
 * 
 * This script and all related assets fall under the CC BY-NC-SA 4.0 License
 * All future derivations of this code should contain the above attribution
 **/

using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;

/// <summary>
/// Processing class for OpenPose ML model's output from heatmaps to actual joint positions
/// Many of these values/structures are hard-coded to the structure of the openpose.onnx model provided with this original codebase
/// If another model is used, the structure of this file will need to be tweaked accordingly
/// </summary>
public class OpenPoseOutputProcessor : CSingletonMono<OpenPoseOutputProcessor>
{
    [Header("Threshold Values")]
    public float thre1 = 0.1f;
    public float thre2 = 0.05f;

    // Candidates (potential points)
    private List<List<Candidate>> all_peaks = new List<List<Candidate>>();          // A structure of (bodypart_num, [candidate,candidate,...])
    private int peak_counter = 0;                                                   // Count to track the candidate point index
    private List<Candidate> candidates = new List<Candidate>();
    private List<PeopleBody> subset = new List<PeopleBody>();

    // Connections (between joints)
    private List<List<Connection>> all_connections = new List<List<Connection>>();  // A structure of (19, connection_list)
    private List<int> special_k = new List<int>();

    // Connection limb pairs
    private static int[,] limbseq = new int[19, 2] { { 2, 3 } , { 2, 6 }, { 3, 4 }, { 4, 5 }, { 6, 7 }, { 7, 8 }, { 2, 9 }, { 9, 10 },
                   { 10, 11 }, { 2, 12 }, { 12, 13 }, { 13, 14 }, { 2, 1 }, { 1, 15 }, { 15, 17 },
                   { 1, 16 }, { 16, 18 }, { 3, 17 }, { 6, 18 } };


    // Indices that are used to create a 2d vector map
    private static int[,] mapIdx = new int[19, 2]{ { 31, 32 }, { 39, 40 }, { 33, 34 }, { 35, 36 }, { 41, 42 }, { 43, 44 }, { 19, 20 }, { 21, 22 },
                  { 23, 24 }, { 25, 26 }, { 27, 28 }, { 29, 30 }, { 47, 48 }, { 49, 50 }, { 53, 54 }, { 51, 52 },
                  { 55, 56 }, { 37, 38 }, { 45, 46 } };

    /// <summary>
    /// Main processing function
    /// </summary>
    public (List<Candidate>, List<PeopleBody>) ProcessOpenPoseBody(IWorker engine)
    {
        InitValue();

        // First, get output from model and preprocess output
        Tensor heatmaps = engine.PeekOutput("output2"); // dimensions are (1,h,w,19)
        Tensor PAF = engine.PeekOutput("output1");      // dimensions are (1,h,w,38);

        Array3D<float> heatmaps_array = TransposeTensorToArray(heatmaps);
        Array3D<float> paf_array = TransposeTensorToArray(PAF);

        // Part 1 : Find all peaks in the heatmaps (which will eventually represent joint positions)
        FindAllPeaks(heatmaps_array);

        // Part2 : Find all connections between two related body joints using the 2d maps 
        FindAllConnection(paf_array);
      
        // Part3 : Clear the repeated connections and split body joints to separately detected people
        CompressConnectionToPeople();

        // Dispose the data we have finished using
        heatmaps.Dispose();
        PAF.Dispose();

        return (candidates, subset);
    }

    /// <summary>
    /// Intialize all lists and values prior to processing output
    /// </summary>
    private void InitValue()
    {
        all_peaks.Clear();
        peak_counter = 0;
        candidates.Clear();
        subset.Clear();
        all_connections.Clear();
        special_k.Clear();
    }

    #region Preprocess Data
    /// <summary>
    /// Transpose the target matrix, mapping target[1,h,w,c] to image aspect[w,h,c] 
    /// </summary>
    private Array3D<float> TransposeTensorToArray(Tensor target)
    {
        int width = target.width;
        int height = target.height;
        int channels = target.channels;

        Array3D<float> new_array = new Array3D<float>(height, width, channels); // transpose so height = width
       
        for (int w = 0; w < width; w++)
        {
            for (int h = 0; h < height; h++)
            {
                for (int c = 0; c < channels; c++)
                {
                    new_array[w, h, c] =  target[0, h, w, c];
                }
            }
        }

        return new_array;
    }
    #endregion

    #region Find All Peaks
    /// <summary>
    /// Search for all peaks within the heatmap to use as body joint positions
    /// Populates the global <code>all_peaks</code> list
    /// </summary>
    private void FindAllPeaks(Array3D<float> heatmap)
    {
        all_peaks.Clear();
        Array3D<float> gaussian_heatmap = Utils.instance.Gaussian_Filter3d(heatmap, sigma: 3);
        int w = gaussian_heatmap.Width;
        int h = gaussian_heatmap.Height;
        
        for (int part = 0; part < 18; part++)
        {
            float avg = 0;
            List<Candidate> body_part_candidates = new List<Candidate>(); // Stores candidates for this body part
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    bool isPeak = true;
                    float currentPoint = gaussian_heatmap[x, y, part];

                    #region Check Surrounding Points
                    if (currentPoint <= thre1)
                    {
                        isPeak = false;
                    }
                    else if (x > 0 && currentPoint < gaussian_heatmap[x - 1, y, part])      // check left point
                    {
                        isPeak = false;
                    }
                    else if (x < w - 1 && currentPoint < gaussian_heatmap[x + 1, y, part])  // check right point
                    {
                        isPeak = false;
                    }
                    else if (y > 0 && currentPoint < gaussian_heatmap[x, y - 1, part])      // check down point
                    {
                        isPeak = false;
                    }
                    else if (y < h - 1 && currentPoint < gaussian_heatmap[x, y + 1, part])  // check up point
                    {
                        isPeak = false;
                    }
                    #endregion

                    if (isPeak)
                    {
                        Candidate cand = new Candidate(x, y, currentPoint, peak_counter);
                        peak_counter++;
                        body_part_candidates.Add(cand);
                        candidates.Add(cand);
                    }

                    avg += currentPoint;
                }
            }

            all_peaks.Add(body_part_candidates);
        }
    }
    #endregion

    #region Find All Connections
    /// <summary>
    /// Finds all the connections between joints within the candidate peaks
    /// </summary>
    private void FindAllConnection(Array3D<float> PAF)
    {
        Vector2 APixelPoint = new Vector2();
        Vector2 BPixelPoint = new Vector2();

        for (int k = 0; k < mapIdx.GetLength(0); k++)
        {
            Array3D<float> scoremap = GetScoreMap(PAF, (mapIdx[k, 0] - 19, mapIdx[k, 1] - 19));
            List<Candidate> candA = all_peaks[limbseq[k, 0] - 1];   // body part A's candidate list
            List<Candidate> candB = all_peaks[limbseq[k, 1] - 1];   // body part B's candidate list

            int nA = candA.Count;
            int nB = candB.Count;
            int indexA = limbseq[k, 0];
            int indexB = limbseq[k, 1];

            if (nA == 0 || nB == 0)
            {
                special_k.Add(k);
                all_connections.Add(null);
                continue;
            }

            List<Connection_Candidate> connection_candidate = new List<Connection_Candidate>();
            for(int i = 0; i < nA; i++)
            {
                APixelPoint.x = candA[i].x;
                APixelPoint.y = candA[i].y;

                for (int j = 0; j < nB; j++)
                {
                    BPixelPoint.x = candB[j].x;
                    BPixelPoint.y = candB[j].y;
                        
                    Vector2 vec = BPixelPoint - APixelPoint;
                    float norm = Mathf.Max(0.001f, vec.magnitude); 
                    vec = vec.normalized;                   
                    List<Vector2> startend = GetMidPointList(APixelPoint, BPixelPoint, num: 10);

                    // Calculate the dot product of the PAF value and original dir
                    List<float> vec_x = GetVecInPAF(scoremap, 0, startend);
                    List<float> vec_y = GetVecInPAF(scoremap, 1, startend);
                    List<float> score_midpts = Utils.instance.ListAdd(Utils.instance.ListMultiply(vec_x, vec.x), Utils.instance.ListMultiply(vec_y, vec.y));
                    float score_with_dist_prior = Utils.instance.ListSum(score_midpts) / score_midpts.Count + Mathf.Min(0.5f * PAF.Height / norm - 1, 0);

                    bool criterion1 = Utils.instance.CountLarger(score_midpts ,thre2) > (0.8f * score_midpts.Count);
                    bool criterion2 = score_with_dist_prior > 0;
           
                    if (criterion1 && criterion2)
                    {
                        Connection_Candidate cc = new Connection_Candidate(i, j, score_with_dist_prior, score_with_dist_prior + candA[i].score + candB[j].score);
                        connection_candidate.Add(cc);
                    }
                }
            }
            connection_candidate.Sort(SortByDescScore); //Descending sort here
            
            List<Connection> connections = new List<Connection>();
            for(int c = 0; c < connection_candidate.Count; c++)
            {
                int i = connection_candidate[c].index_A;
                int j = connection_candidate[c].index_B;
                float s = connection_candidate[c].score_with_dist_prior;

                if ((!ConnectionHaveA(connections, i)) && (!ConnectionHaveB(connections, j)))
                {
                    Connection con = new Connection(candA[i].ind, candB[j].ind, s, i, j);
                    connections.Add(con);
                    if (connections.Count >= Mathf.Min(nA, nB))
                    {
                        break;
                    }
                }
            }
             
            all_connections.Add(connections);
        }
    }

    /// <summary>
    /// Retrieves score map from original PAF
    /// </summary>
    private Array3D<float> GetScoreMap(Array3D<float> original, (int,int) channels)
    {
        Array3D<float> scoremap = new Array3D<float>(original.Width, original.Height, 2);
        int c1 = channels.Item1;
        int c2 = channels.Item2;

        for (int x = 0; x < original.Height; x++)
        {
            for (int y = 0; y < original.Width; y++)
            {
                scoremap[x, y, 0] = original[x, y, c1];
                scoremap[x, y, 1] = original[x, y, c2];
            }
        }

        return scoremap;
    }

    /// <summary>
    /// Gets <code>num</code> equally spaced points between two given Vector2 points
    /// </summary>
    private List<Vector2> GetMidPointList(Vector2 start, Vector2 end, int num = 10)
    {
        List<Vector2> midPointList = new List<Vector2>();
        float x_space = end.x - start.x;
        float y_space = end.y - start.y;
        float interval = 1 / ((float)num - 1);

        for (int i = 0; i < num; i++)
        {
            Vector2 m = new Vector2(start.x + i * x_space * interval, start.y + i * y_space * interval);
            midPointList.Add(m);
        }

        return midPointList;
    }

    /// <summary>
    /// Retrieves vector values from the PAF scoremap
    /// </summary>
    private List<float> GetVecInPAF(Array3D<float> scoremap, int index, List<Vector2> startend)
    {
        List<float> vec = new List<float>();

        for (int i = 0; i < startend.Count; i++)
        {
            float value = scoremap[Mathf.RoundToInt(startend[i].x), Mathf.RoundToInt(startend[i].y), index];
            vec.Add(value);
        }

        return vec;
    }

    /// <summary>
    /// Sorting function that sorts connection candidates by score in descending order
    /// </summary>
    private int SortByDescScore(Connection_Candidate a, Connection_Candidate b)
    {
        return b.score_with_dist_prior.CompareTo(a.score_with_dist_prior);
    }

    /// <summary>
    /// Searches for a Connection within list <code>c</code> that contains the A index <code>ind</code>
    /// </summary>
    private bool ConnectionHaveA(List<Connection> c, int ind)
    {
        foreach (var connection in c)
        {
            if (ind == connection.AinBody)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Searches for a Connection within list <code>c</code> that contains the B index <code>ind</code>
    /// </summary>
    private bool ConnectionHaveB(List<Connection> c, int ind)
    {
        foreach (var connection in c)
        {
            if (ind == connection.BinBody)
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #region Calculate People Subsets
    /// <summary>
    /// Given all the joints and connections, compresses the series of connections into individually
    /// detected people
    /// </summary>
    private void CompressConnectionToPeople()
    {
        for (int k = 0; k < mapIdx.GetLength(0); k++)
        {
            if (!special_k.Contains(k))
            {
                List<int> partAs = GetCandidateIdList(all_connections[k], 0);
                List<int> partBs = GetCandidateIdList(all_connections[k], 1);
                int indexA = limbseq[k, 0] - 1;
                int indexB = limbseq[k, 1] - 1;

                for (int i = 0; i < all_connections[k].Count; i++)
                {
                    int found = 0;
                    int[] subset_idx = new int[] { -1, -1 };
                    for (int j = 0; j < subset.Count; j++)
                    {
                        if ((subset[j].body_part[indexA] == partAs[i]) || (subset[j].body_part[indexB] == partBs[i]))
                        {
                            subset_idx[found] = j;
                            found += 1;
                        }

                    }

                    // Merge lists
                    if (found == 1)
                    {
                        int j = subset_idx[0];
                        if (subset[j].body_part[indexB] != partBs[i])
                        {
                            subset[j].body_part[indexB] = partBs[i];
                            subset[j].total_part += 1;
                            subset[j].total_score += candidates[partBs[i]].score + all_connections[k][i].score;
                        }
                    }
                    else if (found == 2)
                    {
                        int j1 = subset_idx[0];
                        int j2 = subset_idx[1];
                        if (NoDupes(subset[j1], subset[j2]))
                        {
                            MergeTwoPersonInfo(subset[j1], subset[j2]);
                            subset[j1].total_score += all_connections[k][i].score;
                            subset.RemoveAt(j2);
                        }
                        else
                        {
                            subset[j1].body_part[indexB] = partBs[i];
                            subset[j1].total_part += 1;
                            subset[j1].total_score += candidates[partBs[i]].score + all_connections[k][i].score;
                        }
                    }
                    else if ((found == 0) && (k < 17))
                    {
                        PeopleBody pb = new PeopleBody();
                        pb.body_part[indexA] = partAs[i];
                        pb.body_part[indexB] = partBs[i];
                        pb.total_part = 2;
                        pb.total_score = candidates[all_connections[k][i].A].score + candidates[all_connections[k][i].B].score + all_connections[k][i].score;
                        subset.Add(pb);
                    }
                }
            }
        }

        // Delete people candidates that don't have enough joints
        DeletePeopleBelowThreshold();
    }


    /// <summary>
    /// Checks that there are no duplicate body parts between the two candidates
    /// </summary>
    private bool NoDupes(PeopleBody p1, PeopleBody p2)
    {
        for(int i = 0; i < p1.body_part.Length; i++)
        {
            if((p1.body_part[i] > 0) && (p2.body_part[i] > 0))
            {
                //this two have conflict
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Merges the joints between two people candidates
    /// </summary>
    private void MergeTwoPersonInfo(PeopleBody p1, PeopleBody p2)
    {
        for (int i = 0; i < p1.body_part.Length; i++)
        {
            p1.body_part[i] += p2.body_part[i] + 1;
        }

        p1.total_score += p2.total_score;
        p1.total_part += p2.total_part;
    }

    /// <summary>
    /// Grabs either all of the a-index or b-index candidates within a connection based on <paramref name="ind"/>
    /// Returns A-index if <paramref name="ind"/> is 0, B-index otherwise
    /// </summary>
    private List<int> GetCandidateIdList(List<Connection> bodypart_list, int ind)
    {
        List<int> parts = new List<int>();

        foreach(Connection c in bodypart_list)
        {
            switch (ind)
            {
                case 0:
                    parts.Add(c.A);
                    break;
                case 1:
                default:
                    parts.Add(c.B);
                    break;
            }
        }

        return parts;
    }

    /// <summary>
    /// Delete people candidates that don't have enough joints
    /// </summary>
    public void DeletePeopleBelowThreshold()
    {
        List<PeopleBody> deletelist = new List<PeopleBody>();

        for (int i = 0; i < subset.Count; i++)
        {
            if ((subset[i].total_part <4) || (subset[i].total_score/subset[i].total_part < 0.4f))
            {
                deletelist.Add(subset[i]);
            }
        }

        foreach (var d in deletelist)
        {
            subset.Remove(d);
        }
    }
    #endregion
}

#region Helper Classes
/// <summary>
/// Contains the data for a single candidate joint position
/// </summary>
public class Candidate
{
    private int pixel_x;
    private int pixel_y;
    private float confident_score;
    private int ind_in_body_list;
    public int x => pixel_x;
    public int y => pixel_y;
    public float score => confident_score;
    public int ind => ind_in_body_list;

    /// <summary>
    /// Constructor for a new candidate position
    /// </summary>
    /// <param name="p_x">candidate's pixel position x</param>
    /// <param name="p_y">candidate's pixel position y</param>
    /// <param name="c_score">candidate's confidence score</param>
    /// <param name="ind">candidate's index in the whole list</param>
    public Candidate(int p_x,int p_y, float c_score, int ind)
    {
        pixel_x = p_x;
        pixel_y = p_y;
        confident_score = c_score;
        ind_in_body_list = ind;
    }

}

/// <summary>
/// Comtains the data that describes the connection between two body parts
/// </summary>
public class Connection
{
    private int candA;
    private int candB;
    private float connection_score;
    private int index_in_bodypartA;
    private int index_in_bodypartB;

    public int A => candA;
    public int B => candB;
    public float score => connection_score;
    public int AinBody => index_in_bodypartA;
    public int BinBody => index_in_bodypartB;

    public Connection(int a, int b, float m_score, int m_AinBody, int m_BinBody)
    {
        candA = a;
        candB = b;
        connection_score = m_score;
        index_in_bodypartA = m_AinBody;
        index_in_bodypartB = m_BinBody;
    }
}


/// <summary>
/// Structure used in calculate connection candidates
/// </summary>
public class Connection_Candidate
{
    public int index_A;
    public int index_B;
    public float score_with_dist_prior;
    public float score_total;

    public Connection_Candidate(int a, int b, float scoreWithDist, float m_score_total)
    {
        index_A = a;
        index_B = b;
        score_with_dist_prior = scoreWithDist;
        score_total = m_score_total;
    }
}

/// <summary>
/// A data store for the final subset list for a given person
/// </summary>
public class PeopleBody
{
    public int[] body_part = new int[18] { -1, -1, -1, -1, -1, -1, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
    public float total_score;
    public float total_part;
}

#endregion