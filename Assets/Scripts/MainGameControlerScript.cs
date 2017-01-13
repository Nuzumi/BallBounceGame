using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Assets.Scripts;

public class MainGameControlerScript : MonoBehaviour {

    public Shader shader;
    public int searchingWidth;
    public int goalHeight;
    public int width;
    public int height;
    public Vector2 fromPosition;
    public GameObject nodePrefab;
    public Text turnInfotext;
    public Text winText;
    public Color player1Color;
    public Color player2Color;
    public Canvas winCanvas;

    private GameObject nodeExclude;
    private GameObject startNode;
    private GameObject oldStartNode;
    private GameObject winingMidleNode;
    private List<GameObject> allNodesList;
    private List<GameObject> pathMain;
    private bool once;
    private bool isAi;
    private bool isCurrentPlayer1;
    private int player1Goalx;


    private void Awake()
    {

        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
               Instantiate(nodePrefab, new Vector3(fromPosition.x+i,fromPosition.y-j), Quaternion.identity);
            }
        }
        makeGoal();

        allNodesList = new List<GameObject>();
        packAllListAndMskeframeOfCanBounce();
    }

    private void Start()
    {
        once = false;
        isCurrentPlayer1 = true;
        turnInfotext.text = "PLAYER1";
        turnInfotext.color = player1Color;
        player1Goalx = (int)fromPosition.x ;
        winCanvas.enabled = false;
        GameObject[] infoArray = GameObject.FindGameObjectsWithTag("InfoObject");
        pathMain = new List<GameObject>();
        nodeExclude = new GameObject();
        isAi =infoArray[0].GetComponent<InfoObjectScript>().isAi;
        //isAi = true;

    }


    private void Update()
    {
        if (!once)
        {
            once = true;
            startNode.SendMessage("changeColorTo", player1Color);
            startNode.SendMessage("makeNeighbourShine", true);
            drawFrame();
            winingMidleNode = findWining();
        }
    }

    void getNewStartNode(GameObject newStartNode)
    {
        oldStartNode = startNode;
        startNode.SendMessage("makeNeighbourShine", false);
        startNode.SendMessage("deletePathTo", newStartNode);
        startNode = newStartNode;
        oldStartNode.SendMessage("makeItBounce");
        startNode.SendMessage("deletePathTo", oldStartNode);
        canBeThereNextMove();
        didSmbWonGame();
        startNode.SendMessage("makeNeighbourShine", true);
        oldStartNode.SendMessage("changeColorTo", Color.white);
        drawLine(oldStartNode.transform.position, startNode.transform.position, Color.black,false);

        if (isAi)
        {
            aiMoveVer3();
        }
        else
        {
            setColor();
        }
        
       
    }

    void getNewStartNode(List<GameObject> path,bool toEnd)
    {
        
        for(int i = 0; i < path.Count; i++)
        {
            oldStartNode = startNode;

            startNode.SendMessage("makeNeighbourShine", false);
            startNode.SendMessage("deletePathTo", path[i]);
            startNode = path[i];
            oldStartNode.SendMessage("makeItBounce");
            oldStartNode.SendMessage("changeColorTo", Color.white);
            startNode.SendMessage("deletePathTo", oldStartNode);
            drawLine(oldStartNode.transform.position, startNode.transform.position, Color.black,false);
        }

        if (!toEnd)
        {
            makeMove();
        }
        else
        {
            startNode.SendMessage("makeNeighbourShine", true);
            startNode.SendMessage("changeColorTo", player1Color);
            isCurrentPlayer1 = true;
            turnInfotext.text = "PLAYER1";
            turnInfotext.color = player1Color;
        }
        
    }

    void drawLine(Vector3 start, Vector3 end, Color color,bool fade)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        // lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
        lr.material = new Material(shader);
        lr.SetColors(color, color);
        lr.SetWidth(0.1f, 0.1f);
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        if (fade)
        {
            Destroy(myLine, 2.5f);
        }
    } // uzupelnic shader

    void packAllListAndMskeframeOfCanBounce()
    {
        bool foundInIf = false;
        GameObject[] tagArray = GameObject.FindGameObjectsWithTag("Node");

        for (int i = 0; i < tagArray.Length; i++)
        {
            tagArray[i].gameObject.GetComponent<NodeOfGraphScript>().mainControler = gameObject;
            allNodesList.Add(tagArray[i]);

            if (tagArray[i].transform.position == new Vector3(0, 0, 0))
            {             
                startNode = tagArray[i];
            }

            if((tagArray[i].transform.position.y == fromPosition.y || tagArray[i].transform.position.y==fromPosition.y-(height-1) || tagArray[i].transform.position.x == fromPosition.x || tagArray[i].transform.position.x == fromPosition.x+(width-1)))
            {
                if ((tagArray[i].transform.position.x == fromPosition.x || tagArray[i].transform.position.x == fromPosition.x + (width - 1)))
                {
                    foundInIf = false;
                    
                    for(int j = 1; j < goalHeight-1; j++)
                    {
                        if (tagArray[i].transform.position.y == (goalHeight / 2) - j)
                        {
                            foundInIf = true;
                        }
                    }

                    if (!foundInIf)
                    {
                        tagArray[i].gameObject.GetComponent<NodeOfGraphScript>().canBounce = true;
                    }

                    if (foundInIf)
                    {
                        tagArray[i].gameObject.GetComponent<NodeOfGraphScript>().isWining = true;
                    }
                }else
                tagArray[i].gameObject.GetComponent<NodeOfGraphScript>().canBounce = true;
            }

            if((tagArray[i].transform.position.x == fromPosition.x-1 || tagArray[i].transform.position.x == fromPosition.x + width))
            {
                tagArray[i].gameObject.GetComponent<NodeOfGraphScript>().canBounce = true;
            }
        }
    }

    void makeGoal()
    {
        Vector2 startPosition = new Vector2((width+1)/2,goalHeight/2);

        for(int i = 0;i < goalHeight; i++)
        {
            Instantiate(nodePrefab, new Vector3(startPosition.x,startPosition.y - i), Quaternion.identity);
            Instantiate(nodePrefab, new Vector3(-startPosition.x, startPosition.y - i), Quaternion.identity);
        }
    }

    void drawFrame()
    {
        drawLine(fromPosition, new Vector3(fromPosition.x + width - 1, fromPosition.y), Color.black,false);
        drawLine(fromPosition * -1, new Vector3(fromPosition.x + width - 1, fromPosition.y) * -1, Color.black, false);
        drawLine(fromPosition, new Vector3(fromPosition.x, goalHeight/2),Color.black, false);
        drawLine(fromPosition*-1, new Vector3(fromPosition.x, goalHeight / 2)*-1, Color.black, false);
        drawLine(new Vector3(fromPosition.x + width - 1, fromPosition.y), new Vector3(fromPosition.x + width - 1, goalHeight / 2), Color.black, false);
        drawLine(new Vector3(fromPosition.x + width - 1, fromPosition.y)*-1, new Vector3(fromPosition.x + width - 1, goalHeight / 2)*-1, Color.black, false);
        drawLine(new Vector2(fromPosition.x - 1, goalHeight / 2), new Vector2(fromPosition.x - 1, goalHeight / -2), Color.black, false);
        drawLine(new Vector2(fromPosition.x - 1, goalHeight / 2)*-1, new Vector2(fromPosition.x - 1, goalHeight / -2)*-1, Color.black, false);
        drawLine(new Vector2(fromPosition.x - 1, goalHeight / 2), new Vector2(fromPosition.x, goalHeight / 2), Color.black, false);
        drawLine(new Vector2(fromPosition.x - 1, goalHeight / 2)*-1, new Vector2(fromPosition.x, goalHeight / 2)*-1, Color.black, false);
        drawLine(new Vector2(fromPosition.x - 1, goalHeight / -2), new Vector2(fromPosition.x, goalHeight / -2), Color.black, false);
        drawLine(new Vector2(fromPosition.x - 1, goalHeight / -2)*-1, new Vector2(fromPosition.x, goalHeight / -2)*-1, Color.black, false);
    }

    void setColor()
    {
        if (!startNode.GetComponent<NodeOfGraphScript>().canBounce)
        {
            if (isCurrentPlayer1)
            {
                startNode.SendMessage("changeColorTo", player2Color);
                isCurrentPlayer1 = false;
                turnInfotext.text = "PLAYER2";
                turnInfotext.color = player2Color;
              //  drawLine(oldStartNode.transform.position, startNode.transform.position, player1Color);
            }
            else
            {
                startNode.SendMessage("changeColorTo", player1Color);
                isCurrentPlayer1 = true;
                turnInfotext.text = "PLAYER1";
                turnInfotext.color = player1Color;
              //  drawLine(oldStartNode.transform.position, startNode.transform.position, player2Color);
            }
        }
        else
        {
            if (isCurrentPlayer1)
            {
               startNode.SendMessage("changeColorTo", player1Color);
              // drawLine(oldStartNode.transform.position, startNode.transform.position, player1Color);
            }
            else
            {
                startNode.SendMessage("changeColorTo", player2Color);
               // drawLine(oldStartNode.transform.position, startNode.transform.position, player2Color);
            }
        }
    }

    void didSmbWonGame()
    {
        if (startNode.GetComponent<NodeOfGraphScript>().isWining)
        {
            startNode.GetComponent<NodeOfGraphScript>().neighbourhoodNoodeList = new List<GameObject>();

            winCanvas.enabled = true;
            if(startNode.transform.position.x == player1Goalx)
            {
                winText.text = "PLAYER1 WON";
            }
            else
            {
                winText.text = "PLAYER2 WON";
            }
        }
    }

    void makeRandomMove()
    {
        List<GameObject> nodeNeighboutList = startNode.GetComponent<NodeOfGraphScript>().neighbourhoodNoodeList;
        nodeNeighboutList.RemoveAll(thereIsNoWayOut);

        if(nodeNeighboutList.Count != 0)
        {           
           getNewStartNode(nodeNeighboutList[Random.Range(0, nodeNeighboutList.Count)]); 
        }
    }

    void makeRandomMove(int var)
    {
        List<GameObject> nodeList = makeListOfNotBouncing(startNode);
        nodeList.RemoveAll(thereIsNoWayOut);

        if(nodeList.Count != 0)
        {
            getNewStartNode(nodeList[Random.Range(0, nodeList.Count)]);
        }
        else
        {
            makeRandomMove();
        }
        

    }

    void makeMove()
    {
        List<GameObject> nodeList = makeListOfNotBouncing(startNode);
        GameObject bestNode =findBestNode(startNode, nodeList, true);

        if (nodeList.Count == 1)
        {
            getNewStartNode(nodeList[0]);
        }
        else
        {         
            if (bestNode!=null)
            {
                getNewStartNode(bestNode);
            }
            else
            {
                Debug.Log("null noz cholera");
                makeRandomMove(0);
            }
        }
       
        
        
    }

    void makeMoveVer2()
    {
        List<GameObject> nodeList = makeListOfNotBouncing(startNode);
        List<GameObject> bouncingNodeList = startNode.GetComponent<NodeOfGraphScript>().bouncingNeighbourList;
        List<GameObject> potencialePath = new List<GameObject>();
        GameObject bestNode = null;
        bool was = false;

        for(int i = pathMain.Count-1; i != 0; i--)
        {
            if (nodeList.Contains(pathMain[i]))
            {
                bestNode = pathMain[i];
            }
        }

        if(bestNode == null)
        {
            Debug.Log("null no zesz");
            makeMove();
        }
        else
        {

            for(int i = 0; i< bouncingNodeList.Count && !was;i++)                      //each(GameObject elem in bouncingNodeList)
            {
                List<GameObject> afterBounseNodeList = makeListOfNotBouncing(bouncingNodeList[i]);

                for (int j = pathMain.Count - 1; j != 0; j--)
                {
                    if (afterBounseNodeList.Contains(pathMain[j]))
                    {
                        bestNode = pathMain[j];
                        potencialePath.Add(bouncingNodeList[i]);
                        potencialePath.Add(pathMain[j]);
                        was = true;
                    }
                }
            }


            if (!was)
            {
                Debug.Log("miejsce z pirwszego");
                //przydalo by sie sprawdzic czy moge tam isc przez odbicie // no i jest
                foreach (GameObject elem in bouncingNodeList)
                {
                    if (makeListOfNotBouncing(elem).Contains(bestNode))
                    {
                        if (was)
                        {
                            potencialePath.Clear();
                        }

                        was = true;
                        potencialePath.Add(elem);
                    }
                }
                potencialePath.Add(bestNode);
            }

            if (potencialePath.Count == 2)
            {
                Debug.Log("potencial path");
                getNewStartNode(potencialePath, true);
            }
            else
            {
               getNewStartNode(bestNode);
            }
            

        }


    }

    bool isUsingPathGood(GameObject bestPathOutcome,GameObject nodeToStart)
    {
        GameObject pathOutcome = findBestNode(bestPathOutcome, makeListOfNotBouncing(bestPathOutcome), true);
        GameObject nodeOutcome = findBestNode(nodeToStart, makeListOfNotBouncing(nodeToStart), true);
        if (pathOutcome == null)
        {
            pathOutcome = bestPathOutcome;
        }

        if(nodeOutcome == null)
        {
            nodeOutcome = nodeToStart;
        }

        return (distanceToWin(pathOutcome) < distanceToWin(nodeOutcome));

    }

    void aiMove()
    {
        if (!startNode.GetComponent<NodeOfGraphScript>().canBounce)
        {
            if (isCurrentPlayer1)
            {
                startNode.SendMessage("changeColorTo", player2Color);
                isCurrentPlayer1 = false;
                turnInfotext.text = "PLAYER2";
                turnInfotext.color = player2Color;
                computePath(startNode,false,true);
                makeMoveMediumForNow();
            }
            else
            {
                startNode.SendMessage("changeColorTo", player1Color);
                isCurrentPlayer1 = true;
                turnInfotext.text = "PLAYER1";
                turnInfotext.color = player1Color;
            }
        }
        else
        {
            if (isCurrentPlayer1)
            {
                startNode.SendMessage("changeColorTo", player1Color);
            }
            else
            {
                startNode.SendMessage("changeColorTo", player2Color);
                makeMoveMediumForNow();
            }
        }
    }

    void aiMoveVer2()
    {
        if (!startNode.GetComponent<NodeOfGraphScript>().canBounce)
        {
            if (isCurrentPlayer1)
            {
                startNode.SendMessage("changeColorTo", player2Color);
                isCurrentPlayer1 = false;
                turnInfotext.text = "PLAYER2";
                turnInfotext.color = player2Color;
                computePath(startNode,false,true);
                GameObject bestNode = findBestNode(startNode, makeListOfEveryNodeOfPath(),true);
                List <GameObject> path = getPath(bestNode);
                if (path.Count > 1)
                {
                    Debug.Log("path can be used");
                    if (isUsingPathGood(bestNode,startNode))
                    {
                        Debug.Log("path used");
                        path.RemoveAt(0);
                        getNewStartNode(path,false);
                    }else
                    {
                        Debug.Log("path not used");
                        makeMove();
                    }
                    
                }else
                {
                    makeMove();
                }

            }
            else
            {
                startNode.SendMessage("changeColorTo", player1Color);
                isCurrentPlayer1 = true;
                turnInfotext.text = "PLAYER1";
                turnInfotext.color = player1Color;
            }
        }
        else
        {
            if (isCurrentPlayer1)
            {
                startNode.SendMessage("changeColorTo", player1Color);
            }
            else
            {
                startNode.SendMessage("changeColorTo", player2Color);
                makeRandomMove(0);
            }
        }
    }

    void aiMoveVer3()
    {
        if (!startNode.GetComponent<NodeOfGraphScript>().canBounce)
        {
            if (isCurrentPlayer1)
            {
                startNode.SendMessage("changeColorTo", player2Color);
                isCurrentPlayer1 = false;
                turnInfotext.text = "PLAYER2";
                turnInfotext.color = player2Color;
                GameObject someNodeToDoSth = findWhereToGo();
                if (someNodeToDoSth != null)
                {
                    getNewStartNode(getPath(someNodeToDoSth),false);
                }
                else
                {
                    makeMoveVer2();
                }

            }
            else
            {
                startNode.SendMessage("changeColorTo", player1Color);
                isCurrentPlayer1 = true;
                turnInfotext.text = "PLAYER1";
                turnInfotext.color = player1Color;
            }
        }
        else
        {
            if (isCurrentPlayer1)
            {
                startNode.SendMessage("changeColorTo", player1Color);
            }
            else
            {
                startNode.SendMessage("changeColorTo", player2Color);
                //makeRandomMove(0);
                makeMoveVer2();
            }
        }
    }

    bool thereIsNoWayOut(GameObject toFindOut)
    {
        return toFindOut.GetComponent<NodeOfGraphScript>().neighbourhoodNoodeList.Count == 0;
    }

    void canBeThereNextMove()
    {
        if (thereIsNoWayOut(startNode))
        {
            startNode.GetComponent<NodeOfGraphScript>().neighbourhoodNoodeList = new List<GameObject>();
            winCanvas.enabled = true;
            winText.text = "There is no more moves";
        }
    }

    float distanceToWin(GameObject nodeToCheck)
    {
        float minDistance = distance(nodeToCheck.transform.position,new Vector3((width-1)/2,(goalHeight-3)/2));

        for(int i = 0; i < (goalHeight - 3); i++)
        {
            float tmpDistance = distance(nodeToCheck.transform.position, new Vector3((width - 1) / 2, ((goalHeight - 3) / 2)-i));
            if (minDistance > tmpDistance)
            {
                minDistance = tmpDistance;
            }

        }
        return minDistance;
    }

    float distanceToWinPlayer(GameObject nodeToCheck)
    {
        float minDistance = distance(nodeToCheck.transform.position, new Vector3((width - 1) / -2, (goalHeight - 3) / 2));

        for (int i = 0; i < (goalHeight - 3); i++)
        {
            float tmpDistance = distance(nodeToCheck.transform.position, new Vector3((width - 1) / -2, ((goalHeight - 3) / 2) - i));
//            Debug.Log("minDistance:" + minDistance + " tmpDistance:" + tmpDistance);
            if (minDistance > tmpDistance)
            {
                minDistance = tmpDistance;
            }

        }
        return minDistance;
    }

    float distance(Vector3 start ,Vector3 end)
    {
        return Mathf.Sqrt(Mathf.Pow(start.x - end.x, 2) + Mathf.Pow(start.y - end.y, 2));
    }

    void makeMoveMediumForNow()
    {
        List<GameObject> neighbourClosestToWinList = new List<GameObject>();

        if (startNode.GetComponent<NodeOfGraphScript>().neighbourhoodNoodeList.Count != 0)
        {
            

            List<GameObject> neighbourNodeList = startNode.GetComponent<NodeOfGraphScript>().neighbourhoodNoodeList;

           // neighbourClosestToWinList.Add(neighbourNodeList[0]);
            float minDistance = distanceToWin(startNode); // inna wersja distanceToWin(neighbourNodeList[0])

 //   tam gdzie komentarze inna wersja (wybieraja )

            // robie liste nodami sasiadami z najmniejszym dystansem do wygranej 
            for (int i = 0; i < neighbourNodeList.Count; i++)
            {
                if (minDistance >= distanceToWin(neighbourNodeList[i]) && !thereIsNoWayOut(neighbourNodeList[i]))
                {
                   // minDistance = distanceToWin(neighbourNodeList[i]);
                   // neighbourClosestToWinList.Clear();
                    neighbourClosestToWinList.Add(neighbourNodeList[i]);
                }
               /*
                * else
                {
                    if (minDistance == distanceToWin(neighbourNodeList[i]) && !thereIsNoWayOut(neighbourNodeList[i]))
                    {
                        neighbourClosestToWinList.Add(neighbourNodeList[i]);
                    }
                }
                */
            }

            if(neighbourClosestToWinList.Count == 0)
            {
                neighbourClosestToWinList = neighbourNodeList;
            }
            //getNewStartNode(neighbourClosestToWinList[neighbourClosestToWinList.Count-1]);
            
            if (neighbourClosestToWinList.Count == 1)
            {
               // Debug.Log("wyjscie z neighbourClosestToWinList.Count == 1");
                getNewStartNode(neighbourClosestToWinList[0]);//chwilowo
            }
            else
            {
                if (listOfBouncing(neighbourClosestToWinList).Count != 0)
                {
                   // Debug.Log("wjccie z listOfBouncing(neighbourClosestToWinList)[random]  + bylo " + listOfBouncing(neighbourClosestToWinList).Count);
                    getNewStartNode(listOfBouncing(neighbourClosestToWinList)[Random.Range(0,listOfBouncing(neighbourClosestToWinList).Count)]);//mozna udoskonalic i to mocno
                }
                else
                {
                    //lista z nodami tak ze przeciwnik nie ma mozliwosci odbicia sie
                    List<GameObject> nodeListWithNoBounceAfterMove = makeListOfNodeWithNoBounceMoveAther(neighbourClosestToWinList);

                    if (nodeListWithNoBounceAfterMove.Count == 0)
                    {
                       // Debug.Log("wyjscie z neighbourClosestToWinList");
                        if (neighbourClosestToWinList.Count == 1)
                        {
                            getNewStartNode(neighbourClosestToWinList[0]);
                        }else
                        {
                            makeMoveThatGetCloseSimple(neighbourClosestToWinList);// mozna udoskonalic
                        }
                    }
                    else
                    {
                       // Debug.Log("wyjscie z nodeListWithNoBounceAfterMove");
                        if(nodeListWithNoBounceAfterMove.Count == 1)
                        {
                            getNewStartNode(nodeListWithNoBounceAfterMove[0]);
                        }
                        else
                        {
                            makeMoveThatGetCloseSimple(nodeListWithNoBounceAfterMove); // mozna udoskonalic
                        }
                    }                
                }
            }            
        }
        else
        {
            //Debug.Log("wyjscie z randomMove()");
            //tak tylko na teraz
            makeRandomMove();
        }
    }

    List<GameObject> listOfBouncing(List<GameObject> nodeList)
    {
        List<GameObject> listOfBouncing = new List<GameObject>();
        for(int i = 0; i < nodeList.Count; i++)
        {
            if (nodeList[i].GetComponent<NodeOfGraphScript>().canBounce)
            {
                listOfBouncing.Add(nodeList[i]);
            }
        }

        return listOfBouncing;
    } 

    List<GameObject> listOfBouncingWithoutNode(List<GameObject> nodeList,GameObject nodeToExclude)
    {
        List<GameObject> listOfBouncing = new List<GameObject>();
        for (int i = 0; i < nodeList.Count; i++)
        {
            if (nodeList[i].GetComponent<NodeOfGraphScript>().canBounce && nodeList[i] != nodeToExclude)
            {
                listOfBouncing.Add(nodeList[i]);
            }
        }

        return listOfBouncing;
    }

    List<GameObject> listOfBouncingNodesExcludingList(List<GameObject> nodeList,List<GameObject> listToExclude)
    {
        List<GameObject> listOfBouncing = new List<GameObject>();

        for(int i = 0; i < nodeList.Count; i++)
        {
            if(nodeList[i].GetComponent<NodeOfGraphScript>().canBounce && !nodeList[i].GetComponent<NodeOfGraphScript>().predicted && !listToExclude.Contains(nodeList[i]))
            {
                nodeList[i].SendMessage("turnOnPred");
                listOfBouncing.Add(nodeList[i]);
            }
        }

        return listOfBouncing;
    }//tylko do oceniania

    void makeMoveThatGetCloseSimple(List<GameObject> nodeList)
    {
        GameObject bestNode = nodeList[0];
        float bestDistance = distanceToWin(bestNode);

        for(int i = 1; i < nodeList.Count; i++)
        {
            if (bestDistance > distanceToWin(nodeList[i]))
            {
                bestDistance = distanceToWin(nodeList[i]);
                bestNode = nodeList[i];
            }
        }

        getNewStartNode(bestNode);
    } // mozna udoskonalic

    List<GameObject> makeListOfNodeWithNoBounceMoveAther(List<GameObject> nodeList)
    {
        List<GameObject> thatList = new List<GameObject>();
        for (int i = 0; i < nodeList.Count; i++)
        {
            if (listOfBouncingWithoutNode(nodeList[i].GetComponent<NodeOfGraphScript>().neighbourhoodNoodeList,nodeList[i]).Count == 0)
            {
                thatList.Add(nodeList[i]);
            }
        }
        return thatList;
    }

    List<GameObject> makeListOfNodeWithNoBounceMOveAfterOnRight(List<GameObject> nodeList,GameObject nodeToStart)
    {
        List<GameObject> thatList = new List<GameObject>();
        for (int i = 0; i < nodeList.Count; i++)
        {
            if (listOfBouncingWithoutNode(nodeList[i].GetComponent<NodeOfGraphScript>().neighbourhoodNoodeList, nodeList[i]).Count == 0 && nodeList[i].transform.position.x>nodeToStart.transform.position.x)
            {
                thatList.Add(nodeList[i]);
            }
        }
        return thatList;
    }

    List<GameObject> getEndListOfBounce(GameObject bounceNode, List<GameObject> alreadyVisitedNodes)//zwraca liste obiektow na koncu
    {
        bounceNode.SendMessage("turnOnPred");
        alreadyVisitedNodes.Add(bounceNode);
        List<GameObject> bouncingNodesList = listOfBouncingNodesExcludingList(bounceNode.GetComponent<NodeOfGraphScript>().neighbourhoodNoodeList,alreadyVisitedNodes);
        List<GameObject> endList = new List<GameObject>();

        if (bouncingNodesList.Count == 0)
        {
            endList.Add(bounceNode);
            //return endList;
            return alreadyVisitedNodes;
        }
        else
        {
            for (int i = 0; i < bouncingNodesList.Count; i++)
            {
                    dodajListy(alreadyVisitedNodes, getEndListOfBounce(bouncingNodesList[i],alreadyVisitedNodes));
            }
            return alreadyVisitedNodes;
        }
        
    }

    List<GameObject> dodajListy(List<GameObject> first, List<GameObject> secound)
    {
        for(int i = 0; i < secound.Count; i++)
        {
            first.Add(secound[i]);
        }

        return first;
    }

    List<GameObject> makeListOfNotBouncing(GameObject node)
    {
        List<GameObject> nodeList = new List<GameObject>();
        foreach(GameObject elem in node.GetComponent<NodeOfGraphScript>().neighbourhoodNoodeList)
        {
            if (!elem.GetComponent<NodeOfGraphScript>().canBounce)
            {
                nodeList.Add(elem);
            }
        }
        return nodeList;
    }

    void debList(List<GameObject> list)
    {
        Debug.Log("wyswietlanie listy " + list.ToString());
        for(int i = 0; i < list.Count; i++)
        {
            if(list[i] == null)
            {
                Debug.Log("null");
            }
            else
            {
                Debug.Log(list[i].transform.position);
            }
        }
        Debug.Log("koniec");
    }

    void turnOffAllNodesPredictions(GameObject elementToExclude)
    {
        foreach(GameObject elem in allNodesList)
        {
            elem.SendMessage("turnOfPred",elementToExclude);
        }
    }

    void computePath(GameObject source,bool excludeSource,bool bouncing)
    {
        if (excludeSource)
        {
            turnOffAllNodesPredictions(source);
        }
        else
        {
            turnOffAllNodesPredictions(nodeExclude);
        }
        source.SendMessage("setMinDist",0.0f);

      //  Queue<GameObject> nodeQueue = new Queue<GameObject>();
        PriorityQueue nodePriorityQueue = new PriorityQueue();
        nodePriorityQueue.enqueue(source);
       // nodeQueue.Enqueue(source);

        while(nodePriorityQueue.count() != 0)
        {
            GameObject someNode = nodePriorityQueue.dequeue();
            List<GameObject> nodeList = new List<GameObject>();

            if (bouncing)
            {
                nodeList = someNode.GetComponent<NodeOfGraphScript>().bouncingNeighbourList;
            }
            else
            {
                nodeList = someNode.GetComponent<NodeOfGraphScript>().neighbourhoodNoodeList;
            }

            foreach(GameObject elem in nodeList)
            {
                GameObject diffrentNode = elem;
                float distanceThroughN = someNode.GetComponent<NodeOfGraphScript>().minDist + someNode.GetComponent<NodeOfGraphScript>().weight;
                if (distanceThroughN < diffrentNode.GetComponent<NodeOfGraphScript>().minDist)
                {
                    nodePriorityQueue.remove(diffrentNode);// = removeFromQueue(nodeQueue, diffrentNode);
                    diffrentNode.SendMessage("setMinDist",distanceThroughN);
                    diffrentNode.SendMessage("setPrevious", someNode);
                    nodePriorityQueue.enqueue(diffrentNode);
                }
            }
        }
    }

    List<GameObject> getPath(GameObject target)
    {
        List<GameObject> path = new List<GameObject>();

        for (GameObject i = target; i != null; i = i.GetComponent<NodeOfGraphScript>().previous)
        {
            path.Add(i);
        }

        path.Reverse();
        pathMain = path;
        return path;
    }

    Queue<GameObject> removeFromQueue(Queue<GameObject> queue,GameObject nodeToRemove)
    {
        Queue<GameObject> newQueue = new Queue<GameObject>();

        foreach(GameObject elem in queue)
        {
            if(elem != nodeToRemove)
            {
                newQueue.Enqueue(elem);
            }
        }

        return newQueue;
    }

    List<GameObject> makeListOfEveryNodeOfPath()
    {
        List<GameObject> nodeList = new List<GameObject>();

        foreach(GameObject elem in allNodesList)
        {
            if(elem.GetComponent<NodeOfGraphScript>().previous != null)
            {
                nodeList.Add(elem);
            }
        }
        return nodeList;
    }

    GameObject findBestNode(GameObject nodeToStart,List<GameObject> nodeList,bool forAi)
    {
        GameObject bestNode = null;
        float distance;

        if (forAi)
        {
            distance = distanceToWin(nodeToStart);
            foreach (GameObject elem in nodeList)
            {
                float distanceToShow = distanceToWin(elem); 
                if ((makeListOfNotBouncing(elem).Count > 1 && distance > distanceToShow) || (elem.GetComponent<NodeOfGraphScript>().isWining  && elem.transform.position.x>0))
                {
                    distance = distanceToShow;
                    bestNode = elem;
                }
            }
        }
        else
        {
            distance = distanceToWinPlayer(nodeToStart);
            foreach (GameObject elem in nodeList)
            {
                float distanceToShow = distanceToWinPlayer(elem);
                Debug.Log("minDistance: " + distance + " Distance: " + distanceToShow);
                if ((makeListOfNotBouncing(elem).Count > 1 && distance > distanceToShow) || (elem.GetComponent<NodeOfGraphScript>().isWining && elem.transform.position.x < 0))
                {
                    distance = distanceToShow;
                    bestNode = elem;
                }
            }
        }

        return bestNode;

    }

    GameObject findWhereToGo() // siatka na cala mape i szukanie jak daleko moge isc // ale i wyznaczanie drogi iscia
    {
        computePath(startNode, false, false);//cala siatka
        List<GameObject> everyNodeOfPathToWin = getPath(winingMidleNode);
        debList(everyNodeOfPathToWin);
        computePath(startNode, false, true);//siatka dostepna
        List<GameObject> whereCanIGo = makeListOfEveryNodeOfPath();
        List<GameObject> listToSearch = new List<GameObject>();

        if (everyNodeOfPathToWin[0] == startNode)
        {
            for (int i = everyNodeOfPathToWin.Count - 1; i != 0; i--)
            {
                // sprawdzac czy i-ty node iest w siatce dostepnej
                // jak tak to zwrocic jak nie bedzie to szukac z wieksza dostepna odlegloscia // do tego chyba napisac nowa funkcej
                if (whereCanIGo.Contains(everyNodeOfPathToWin[i]))
                {
                    return everyNodeOfPathToWin[i];
                }else
                {

                    listToSearch = nodesUpAndDown(everyNodeOfPathToWin[i]);
                    foreach(GameObject elem in listToSearch)
                    {
                        if (whereCanIGo.Contains(elem))
                        {
                            return elem;
                        }
                    }
                    
                    foreach(GameObject elem in listToSearch)
                    {
                        foreach(GameObject element in nodesUpAndDown(elem))
                        {
                            if (whereCanIGo.Contains(element))
                            {
                                return element;
                            }
                        }
                    }

                }
            }
        }

        return null;
    }

    GameObject findWining()
    {
        bool find = false;
        GameObject node = startNode;
        List<GameObject> nodeList = node.GetComponent<NodeOfGraphScript>().neighbourhoodNoodeList;

        for (int i = 0; !find;i++)
        {
            if((nodeList[i].transform.position.x -1) == node.transform.position.x && nodeList[i].transform.position.y == node.transform.position.y)
            {
                node = nodeList[i];
                if (node.GetComponent<NodeOfGraphScript>().isWining)
                {
                    return node;
                }
                nodeList = node.GetComponent<NodeOfGraphScript>().neighbourhoodNoodeList;
                i = 0;
            }
        }
        return node;
    }

    List<GameObject> nodesUpAndDown(GameObject node)
    {
        List<GameObject> finalList = new List<GameObject>();
        List<GameObject> neiList = node.GetComponent<NodeOfGraphScript>().neighbourhoodNoodeList;
        
           for(int i = 0; i < neiList.Count; i++)
           {

               if(neiList[i].transform.position.x == node.transform.position.x 
                                                    &&
                  Mathf.Abs(neiList[i].transform.position.y)-1 == node.transform.position.y)
               {
                   finalList.Add(neiList[i]);
               }
           }
        return finalList;
    }


    /*
    GameObject findBestPredict(GameObject node)
    {

        List<GameObject> nodeList = makeListOfNotBouncing(node);
        GameObject[] BestPlayerMoveTab = new GameObject[nodeList.Count];
        int indeks = 0;
        float odleglosc = (float)double.NegativeInfinity;

        for(int i = 0; i < nodeList.Count; i++)
        {
            nodeList[i].SendMessage("deletePathTo", node);
            computePath(nodeList[i], true);
            List<GameObject> tmpList = makeListOfEveryNodeOfPath();
            debList(tmpList);
            BestPlayerMoveTab[indeks] = findBestNode(nodeList[i], tmpList, false);
            nodeList[i].SendMessage("addPathTo", node);
            indeks++;
        }
        indeks = 0;
        float tmpFloat;

        for(int i = 0; i < nodeList.Count; i++)
        {
            if (BestPlayerMoveTab[i] != null)
            {
                BestPlayerMoveTab[i] = findBestNode(BestPlayerMoveTab[i], makeListOfNotBouncing(BestPlayerMoveTab[i]), false);
                tmpFloat = distanceToWinPlayer(BestPlayerMoveTab[i]);
                if (tmpFloat > odleglosc)
                {
                    Debug.Log("weszlo");
                    odleglosc = tmpFloat;
                    indeks = i;
                }
            }
        }

        return nodeList[indeks];
    }*/ // nie dziala
}