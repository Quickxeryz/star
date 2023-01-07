namespace Classes
{
    public enum Node
    {
        C = 11,
        CH = 10,
        D = 9,
        DH = 8,
        E = 7,
        F = 6,
        FH = 5,
        G = 4,
        GH = 3,
        A = 2,
        AH = 1,
        B = 0
    }
    public static class NodeFunctions
    {
        public static Node getNode(int nodeNumber)
        {
            Node node;
            while (nodeNumber < 0)
            {
                nodeNumber += 12;
            }
            switch (nodeNumber)
            {
                case 0:
                    node = Node.C;
                    break;
                case 1:
                    node = Node.CH;
                    break;
                case 2:
                    node = Node.D;
                    break;
                case 3:
                    node = Node.DH;
                    break;
                case 4:
                    node = Node.E;
                    break;
                case 5:
                    node = Node.F;
                    break;
                case 6:
                    node = Node.FH;
                    break;
                case 7:
                    node = Node.G;
                    break;
                case 8:
                    node = Node.GH;
                    break;
                case 9:
                    node = Node.A;
                    break;
                case 10:
                    node = Node.AH;
                    break;
                case 11:
                    node = Node.B;
                    break;
                default:
                    node = Node.C;
                    break;
            }
            return node;
        }
    }
}
