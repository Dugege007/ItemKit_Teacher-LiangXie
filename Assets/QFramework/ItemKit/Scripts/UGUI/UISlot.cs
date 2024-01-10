using QFramework.Example;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace QFramework
{
    public class UISlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Image Icon;
        //public Text Name;
        public Text Count;

        public Slot Data { get; private set; }

        private bool mDragging = false;

        public UISlot InitWithData(Slot data)
        {
            Data = data;

            if (Data.Count == 0)
            {
                Icon.Hide();
                //Name.text = "空";
                Count.text = "";
            }
            else
            {
                Icon.Show();
                //Name.text = Data.Item.Name;
                Count.text = Data.Count.ToString();
            }

            return this;    // 返回自身，用于链式调用
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (mDragging || Data.Count < 1) return;
            mDragging = true;

            UGUIInventoryExample controller = FindAnyObjectByType<UGUIInventoryExample>();
            Icon.Parent(controller);
            //Name.Parent(controller);
            SyncItemToMousePos();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (mDragging)
            {
                SyncItemToMousePos();
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (mDragging)
            {
                Icon.Parent(transform);
                //Name.Parent(transform);
                // 位置还原（坐标归零）
                Icon.LocalPositionIdentity();
                //Name.LocalPositionIdentity();

                // 检测鼠标是否在任意一个 UISlot 上
                UISlot[] uiSlots = transform.parent.GetComponentsInChildren<UISlot>();

                bool throwItem = true;

                foreach (var uiSlot in uiSlots)
                {
                    RectTransform rectTrans = uiSlot.transform as RectTransform;
                    if (RectTransformUtility.RectangleContainsScreenPoint(rectTrans, Input.mousePosition))
                    {
                        throwItem = false;

                        // 物品交换
                        if (Data.Count > 0)
                        {
                            // 缓存目标栏位数据
                            Item itemCache = uiSlot.Data.Item;
                            int countCache = uiSlot.Data.Count;

                            // 将当前栏位数据保存到目标栏位中
                            uiSlot.Data.Item = Data.Item;
                            uiSlot.Data.Count = Data.Count;

                            // 将目标栏位数据保存到当前栏位中
                            Data.Item = itemCache;
                            Data.Count = countCache;

                            // 刷新
                            FindAnyObjectByType<UGUIInventoryExample>().Refresh();
                        }

                        break;
                    }
                }

                if (throwItem)
                {
                    Data.Item = null;
                    Data.Count = 0;
                    // 刷新
                    FindAnyObjectByType<UGUIInventoryExample>().Refresh();
                }
            }

            mDragging = false;
        }

        // 同步物品到鼠标位置（拖拽时）
        private void SyncItemToMousePos()
        {
            Vector3 mousePos = Input.mousePosition;
            UGUIInventoryExample controller = FindAnyObjectByType<UGUIInventoryExample>();
            // 将屏幕上的点转换为位于指定 RectTransform 平面上的本地坐标系中的位置
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                controller.transform as RectTransform, // rect：目标 RectTransform，将屏幕点转换到这个 RectTransform 定义的平面内
                mousePos,                   // screenPoint：屏幕坐标，通常是鼠标位置或触摸位置
                null,                       // camera：相关联的摄像机，对于 Overlay 画布可以为 null，对于 Camera 和 WorldSpace 画布应该是渲染该 UI 元素的摄像机
                out Vector2 localPos        // localPoint：输出参数，转换后的本地坐标系中的位置
            ))
            {
                // 将物品的名称移动到屏幕上的鼠标位置
                Icon.LocalPosition2D(localPos);
                //Name.LocalPosition2D(localPos);
            }
        }
    }
}
