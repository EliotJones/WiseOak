# Wise Oak #

This is a unmaintained/throwaway project investigating building C# decision trees.

Ideally it could be used to investigate implementing [globally optimal decision trees](https://www.researchgate.net/publication/2784586_Global_Tree_Optimization_A_Non-greedy_Decision_Tree_Algorithm)
but for now it's just used to play around with simple decision trees.

```
var data = new []
{
    // Class 1
    new []{ 0.2, 1.5 },
    new []{ 0.5, 0.2},
    new []{ 0.6, 1.2 },
    new []{ 1.0, 2.3 },
    new []{ 1.8, 0.3 },
    
    // Class 2
    new []{ 2.3, 1.6 },
    new []{ 2.4, 1.4 },
    new []{ 2.5, 3.1 },
    new []{ 2.5, 0.3 },
    new []{ 2.9, 2.1 }
};

var classes = new[]
{
    1, 1, 1, 1, 1,
    2, 2, 2, 2, 2
};

var tree = DecisionTree.Build(data, classes);

Assert.NotNull(tree);
Assert.NotNull(tree.Root);
Assert.False(tree.IsEmpty);

Assert.True(tree.Root.IsLeaf);
Assert.Null(tree.Root.Left);
Assert.Null(tree.Root.Right);

var predictionClass1 = tree.Predict(new[] {0.7, 5});
var predictionClass2 = tree.Predict(new[] {6, 0.3});
```

### References ###

- https://www.kaggle.com/dmilla/introduction-to-decision-trees-titanic-dataset
- https://en.wikipedia.org/wiki/Decision_tree_learning
- https://victorzhou.com/blog/gini-impurity/