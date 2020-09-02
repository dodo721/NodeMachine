namespace NodeMachine.Nodes {

    public struct NodeError {
        
        public string error;
        public string errorFull;
        public Node source;

        public NodeError (string error, string errorFull, Node source) {
            this.error = error;
            this.errorFull = errorFull;
            this.source = source;
        }

    }

}