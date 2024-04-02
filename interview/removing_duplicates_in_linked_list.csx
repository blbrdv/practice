#nullable enable
using System;

public class ListNode 
{
    public int Data;
    public ListNode? Next;

    public ListNode(int data, ListNode? next) 
    {
        this.Data = data;
        this.Next = next;
    }
}

public static class Remover
{
    public static ListNode RemoveDuplicates(ListNode node)
    {
        var data = new List<int>();

        ListNode? currentNode = node;
        do 
        {
            data.Add(currentNode!.Data);

            ListNode? nextNode = currentNode.Next;
            var isDuplicate = true;
            while (nextNode != null && isDuplicate)
            {
                if (data.Contains(nextNode.Data))
                {
                    currentNode.Next = nextNode.Next;
                    isDuplicate = true;
                }
                else
                {
                    data.Add(nextNode!.Data);
                    isDuplicate = false;
                }

                nextNode = nextNode.Next;
                
            }

            currentNode = currentNode!.Next;
        }
        while (currentNode != null);


        return node;
    }
}

var firstList = new ListNode(1, 
    new ListNode(2, 
    new ListNode(2, 
    new ListNode(2, 
    new ListNode(3, null)))));
var newList = Remover.RemoveDuplicates(firstList);

ListNode? nextNode = newList;
do 
{
    Console.WriteLine(nextNode.Data);
    nextNode = nextNode.Next;
}
while (nextNode != null);

#nullable disable
