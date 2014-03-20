using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EPKit.ObjectGraph
{
    public interface IGraphContext
    {
        /// <summary>
        /// Persist one object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        Task Persist<T>(T value) where T : class, new();

        /// <summary>
        /// Remove one object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        Task Remove<T>(T value) where T : class, new();

        /// <summary>
        /// Remove one object by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task RemoveById<T>(string id) where T : class, new();
        
        /// <summary>
        /// Get a collection of items according to the condition
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="condition"></param>
        /// <param name="includes"></param>
        /// <returns></returns>
//        Task<IEnumerable<T>> Get<T>(Expression<Func<T, bool>> condition = null, params Expression<Func<T, object>>[] includes);
        
        /// <summary>
        /// Get one item by Id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="includes"></param>
        /// <returns></returns>
        Task<T> GetById<T>(string id, params Expression<Func<T, object>>[] includes) where T : class, new();
    }
}
