"""
支持向量机模型模块
"""
from sklearn.svm import SVC
from src.utils.config_loader import CONFIG
from src.utils.logger import logger


def train_svm(x_train, y_train, progress_callback=None):
    """
    训练支持向量机模型
    
    Args:
        x_train: 训练特征
        y_train: 训练标签
        progress_callback: 进度回调函数
        
    Returns:
        object: 训练好的模型
    """
    if progress_callback:
        progress_callback("训练SVM模型")
    
    # 获取模型参数
    config = CONFIG['models']['svm']
    kernel = config.get('kernel', 'rbf')
    C = config.get('C', 1.0)
    gamma = config.get('gamma', 'scale')
    random_state = config.get('random_state', 42)
    
    logger.info(f"训练SVM模型: kernel={kernel}, C={C}, gamma={gamma}, random_state={random_state}")
    
    # 初始化模型
    model = SVC(
        kernel=kernel,
        C=C,
        gamma=gamma,
        random_state=random_state,
        probability=True  # 启用概率估计以计算AUC
    )
    
    # 训练模型
    model.fit(x_train, y_train)
    
    if progress_callback:
        progress_callback(20)
    
    logger.info("SVM模型训练完成")
    return model 