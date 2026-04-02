using Cysharp.Threading.Tasks;
using PupSurvivors.Stage;

public interface IItem
{
    UniTaskVoid Obtain(Player target);
}