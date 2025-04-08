"""
逻辑回归模型模块
"""
from sklearn.linear_model import LogisticRegression
from src.utils.config_loader import CONFIG
from src.utils.logger import logger


def train_logistic_regression(x_train, y_train, progress_callback=None):
    """
    训练逻辑回归模型
    
    Args:
        x_train: 训练特征
        y_train: 训练标签
        progress_callback: 进度回调函数
        
    Returns:
        object: 训练好的模型
    """
    if progress_callback:
        progress_callback("训练逻辑回归模型")
    
    # 获取模型参数
    config = CONFIG['models']['logistic_regression']
    C = config.get('C', 1.0)
    max_iter = config.get('max_iter', 100)
    random_state = config.get('random_state', 42)
    
    logger.info(f"训练逻辑回归模型: C={C}, max_iter={max_iter}, random_state={random_state}")
    
    # 初始化模型
    model = LogisticRegression(
        C=C,
        max_iter=max_iter,
        random_state=random_state
    )
    
    # 训练模型
    model.fit(x_train, y_train)
    
    if progress_callback:
        progress_callback(20)
    
    logger.info("逻辑回归模型训练完成")
    return model 