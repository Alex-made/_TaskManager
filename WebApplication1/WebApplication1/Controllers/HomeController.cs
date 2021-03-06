﻿using NHibernate;
using NHibernate.Cfg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;
using WebApplication1.Util;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRepository _repository;

        public HomeController(IRepository repository)
        {
            _repository = repository;
        }       
        //read Tasks
        public JsonResult GetTasks()
        {
            var tasks = _repository.Tasks.ToList().GroupJoin(
                _repository.SubTasks.ToList(),
                x => x.TaskId,
                y => y.TaskId,
                (t, subt) => new TaskModel
                {
                    TaskId = t.TaskId,
                    Header = t.Header,
                    Description = t.Description,
                    CompleteDate = t.CompleteDate,
                    Statuses = Status.getStatuses(t.Status),
                    Priority = t.Priority,
                    SubTasks = subt.Select((subTask)=>
                    new SubTaskModel
                    {
                        SubTaskId = subTask.SubTaskId,
                        Description = subTask.Description,
                        TaskId = subTask.TaskId,
                        Statuses = Status.getStatuses(subTask.Status)
                    }).ToList()
                    //SubTasks = new SubTaskModel
                    //{
                    //    SubTaskId = subt.Select
                    //    Description
                    //    TaskId 
                    //    Statuses 
                    //}.
                    //subt.Select(x=>x).ToList()
                }).ToList();

            return Json(tasks, JsonRequestBehavior.AllowGet);
        }
        //update Taks
        public JsonResult UpdateTask(Task task)
        {
            try
            {
                _repository.UpdateTask(task);
                return Json("Saved!", JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json("Попытка записать пустое значение в базу!", JsonRequestBehavior.AllowGet);
            } 
            
        }


        //update SubTasks
        public void UpdateSubTask(SubTask subTask)
        {
            _repository.UpdateSubTask(subTask);

            // При заверешении подзадачи смотрим есть ли незавершённые подзадачи у родительской
            if (subTask.Status == Status.closeStatus)
            {
                List<SubTask> subTasks = _repository.SubTasks.Where(x => x.Status != Status.closeStatus).ToList();

                // Все подзадачи со статусом Выполнена
                if (subTasks.Count == 0)
                {
                    Task task = _repository.Tasks.Where(x => x.TaskId == subTask.TaskId).FirstOrDefault();
                    task.Status = Status.closeStatus;

                    _repository.UpdateTask(task);

                }
            }







        }




        //delete Tasks
        public void DeleteTask(int TaskId = 0)
        {
            if (TaskId != 0)
            {
                Task task = _repository.Tasks.Where(x => x.TaskId == TaskId).FirstOrDefault();

                _repository.DeleteTask(task);
            }
        }
        //delete subTasks
        public void DeleteSubTask(int SubTaskId = 0)
        {
            if (SubTaskId != 0)
            {
                SubTask subTask = _repository.SubTasks.Where(x => x.SubTaskId == SubTaskId).FirstOrDefault();

                _repository.DeleteSubTask(subTask);
            }
        }

        public ActionResult Index()
        {
            return View();
        }
    }
}
