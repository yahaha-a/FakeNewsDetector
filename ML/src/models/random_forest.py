"""
随机森林模型模块
"""
from sklearn.ensemble import RandomForestClassifier
from sklearn.model_selection import GridSearchCV
from src.utils.config_loader import CONFIG
from src.utils.logger import logger


def train_random_forest(x_train, y_train, progress_callback=None):
    """
    训练随机森林模型
    
    Args:
        x_train: 训练特征
        y_train: 训练标签
        progress_callback: 进度回调函数
        
    Returns:
        object: 训练好的模型
    """
    # 获取模型参数
    config = CONFIG['models']['random_forest']
    n_estimators = config.get('n_estimators', [120, 200, 300])
    max_depth = config.get('max_depth', [5, 8, 15])
    random_state = config.get('random_state', 42)
    cv_folds = CONFIG['evaluation'].get('cv_folds', 5)
    
    logger.info(f"训练随机森林模型: n_estimators={n_estimators}, max_depth={max_depth}, "
               f"random_state={random_state}, cv_folds={cv_folds}")
    
    # 初始化基础模型
    base_model = RandomForestClassifier(random_state=random_state)
    
    # 设置网格搜索参数
    if progress_callback:
        progress_callback("网格搜索优化参数")
    
    param_grid = {
        "n_estimators": n_estimators,
        "max_depth": max_depth
    }
    
    logger.info("开始随机森林模型网格搜索")
    model = GridSearchCV(base_model, param_grid=param_grid, cv=cv_folds)
    
    # 训练模型
    if progress_callback:
        progress_callback("训练随机森林模型")
    
    model.fit(x_train, y_train)
    
    if progress_callback:
        progress_callback(20)
    
    # 输出最佳参数
    logger.info(f"随机森林最佳参数: {model.best_params_}")
    logger.info(f"随机森林最佳交叉验证结果: {model.best_score_:.4f}")
    
    logger.info("随机森林模型训练完成")
    return model 