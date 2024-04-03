# Ssspicy开发文档

## 2024/3/29

实现：简单设置了一个教程关卡的2DTilemap；蛇的基本移动（尾巴Sprite状态未完成）；简单的物体碰撞判定；

Bug：蛇移动时不同方向Sprite闪烁（推测和Animator状态机的问题有关）；

## 2024/3/30

实现：蛇尾移动的状态变更；吃食物延长蛇身的效果和表情变更；吃辣椒向后移动、停止判定、表情变更；蛇掉落判定和表情变更

Bug：昨天的Bug没修；多个道具推到墙边后先吃到墙边的；蛇掉落时渲染图层错误问题；

## 2024/3/31

实现：蛇吃辣椒后沿途物品推动的判定

重构：重构Tilemap，将碰撞体写入Tilemap进行检测

修复：修复“多个道具推到墙边后先吃到墙边的”Bug；修正了后退时的碰撞判定范围，解决了后退停止时可能出现的坐标计算错误

## 2024/4/1

实现：钻洞动画；喷火动画和屏幕震动；

修复：蛇移动时的闪烁问题（修改了状态机）；

## 2024/4/2

实现：星星特效、尘土特效；新增方块-冰块；冰块被喷火破坏；新增两张地图；

重构：将物品父类重构为可移动方块父类；

修复：发现喷火动画方向错误的问题并修复；

Bug：一些渲染顺序遮挡关系存在错误；蛇吃辣椒后不能后退的情况下可能播放不出喷火动画；延迟执行函数期间的操作导致怪异的游戏行为（如：掉落时仍可前进）

## 2024/4/3

实现：新增方块-木墙、沙坑；新增两张地图